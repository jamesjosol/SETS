using System.IO;
using System.Diagnostics;
using SETSDeployer.Models;

namespace SETSDeployer.Services
{
    public class BuildService
    {
        private readonly DeployerSettings _settings;
        private readonly Action<string, LogLevel> _log;

        public BuildService(DeployerSettings settings, Action<string, LogLevel> log)
        {
            _settings = settings;
            _log = log;
        }

        public async Task<bool> BuildFrontendAsync(CancellationToken ct = default)
        {
            var clientDir = Path.Combine(_settings.SolutionRootPath, "sets.client");
            if (!Directory.Exists(clientDir))
            {
                _log($"Frontend directory not found: {clientDir}", LogLevel.Error);
                return false;
            }

            _log("Running npm install...", LogLevel.Info);
            bool installOk = await RunProcessAsync("cmd.exe", "/c npm install", clientDir, ct);
            if (!installOk) { _log("npm install failed.", LogLevel.Error); return false; }
            _log("npm install complete.", LogLevel.Success);

            _log("Running npm run build...", LogLevel.Info);
            bool buildOk = await RunProcessAsync("cmd.exe", "/c npm run build", clientDir, ct);
            if (!buildOk) { _log("npm run build failed.", LogLevel.Error); return false; }
            _log("Frontend build complete.", LogLevel.Success);
            return true;
        }

        public async Task<bool> PublishBackendAsync(CancellationToken ct = default)
        {
            var csproj = Path.Combine(_settings.SolutionRootPath, "SETS.Server", "SETS.Server.csproj");
            if (!File.Exists(csproj))
            {
                _log($"Server project not found: {csproj}", LogLevel.Error);
                return false;
            }

            var args = $"publish \"{csproj}\" --configuration Release --runtime win-x64 --self-contained false --output \"{_settings.PublishOutputPath}\" /p:EnvironmentName=Production";

            _log("Running dotnet publish...", LogLevel.Info);
            bool ok = await RunProcessAsync("dotnet", args, _settings.SolutionRootPath, ct);
            if (!ok) { _log("dotnet publish failed.", LogLevel.Error); return false; }

            // Copy frontend dist into wwwroot
            var distDir = Path.Combine(_settings.SolutionRootPath, "sets.client", "dist");
            var wwwroot = Path.Combine(_settings.PublishOutputPath, "wwwroot");

            if (Directory.Exists(distDir))
            {
                _log("Copying frontend dist → wwwroot...", LogLevel.Info);
                CopyDirectory(distDir, wwwroot);
                _log("Frontend copied to wwwroot.", LogLevel.Success);
            }
            else
            {
                _log("Warning: dist folder not found — skipping wwwroot copy.", LogLevel.Warning);
            }

            _log("Backend publish complete.", LogLevel.Success);
            return true;
        }

        private async Task<bool> RunProcessAsync(string exe, string args, string workDir, CancellationToken ct)
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                WorkingDirectory = workDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var tcs = new TaskCompletionSource<int>();

            proc.OutputDataReceived += (_, e) => { if (e.Data != null) _log(e.Data, LogLevel.Dim); };
            proc.ErrorDataReceived  += (_, e) => { if (e.Data != null) _log(e.Data, LogLevel.Warning); };
            proc.Exited += (_, _) => tcs.TrySetResult(proc.ExitCode);

            ct.Register(() => { try { proc.Kill(); } catch { } tcs.TrySetCanceled(); });

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            int exitCode = await tcs.Task;
            return exitCode == 0;
        }

        private static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(source, file);
                var target = Path.Combine(dest, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, overwrite: true);
            }
        }
    }

    public enum LogLevel { Info, Success, Warning, Error, Dim }
}
