using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using SETSDeployer.Models;

namespace SETSDeployer.Services
{
    public class DeployService
    {
        private readonly DeployerSettings _settings;
        private readonly Action<string, LogLevel> _log;

        private const string APP_OFFLINE = "app_offline.htm";
    private const string APP_OFFLINE_CONTENT = """
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <meta http-equiv="refresh" content="15">
                <title>SETS — Updating</title>
                <style>
                *{box-sizing:border-box;margin:0;padding:0}
                body{min-height:100vh;background-color:#0f1117;display:flex;align-items:center;justify-content:center;font-family:'Segoe UI',system-ui,-apple-system,sans-serif;color:#e8ecf4}
                .container{text-align:center;padding:56px 32px;max-width:500px;width:100%}
                .brand{display:flex;flex-direction:column;align-items:center;gap:10px;margin-bottom:48px}
                .brand-top{display:flex;align-items:center;justify-content:center;gap:12px}
                .brand-name{font-size:22px;font-weight:500;letter-spacing:-0.03em;color:#7c4dff;line-height:1}
                .brand-sub{font-size:9px;font-weight:700;letter-spacing:0.12em;text-transform:uppercase;color:#5a6478;margin-top:2px;line-height:1.5;text-align:center}
                .badge{display:inline-flex;align-items:center;gap:7px;background:#0f2027;color:#4f8ef7;font-size:11px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;padding:6px 14px;border-radius:20px;border:1px solid #1e3a6e;margin-bottom:28px}
                .badge-dot{width:6px;height:6px;background:#4f8ef7;border-radius:50%;animation:pulse 1.2s infinite}
                @keyframes pulse{0%,100%{opacity:1}50%{opacity:0.2}}
                h1{font-size:26px;font-weight:700;color:#e8ecf4;margin-bottom:14px;line-height:1.3}
                .subtitle{font-size:14px;color:#8b95aa;line-height:1.75;margin-bottom:40px}
                .divider{width:40px;height:2px;background:#252d40;margin:0 auto 40px;border-radius:2px}
                .progress-wrap{background:#161b24;border:1px solid #252d40;border-radius:100px;height:6px;overflow:hidden;margin-bottom:40px}
                .progress-bar{height:100%;width:60%;background:linear-gradient(90deg,#4f8ef7,#7c4dff);border-radius:100px;animation:progress 2s ease-in-out infinite alternate}
                @keyframes progress{0%{width:30%}100%{width:85%}}
                .footer{font-size:12px;color:#5a6478;line-height:1.7}
                .footer strong{color:#8b95aa;font-weight:600}
                </style>
                </head>
                <body>
                <div class="container">
                  <div class="brand">
                    <div class="brand-top">
                      <svg width="40" height="40" viewBox="0 0 60 52" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M4 8 L4 36 Q4 46 10 46 Q16 46 16 36 L16 8" stroke="#7c4dff" stroke-width="2" fill="none"/>
                        <line x1="2" y1="8" x2="18" y2="8" stroke="#7c4dff" stroke-width="2.5" stroke-linecap="round"/>
                        <path d="M4 28 L4 36 Q4 46 10 46 Q16 46 16 36 L16 28 Z" fill="#7c4dff" opacity="0.4"/>
                        <path d="M22 4 L22 36 Q22 46 28 46 Q34 46 34 36 L34 4" stroke="#7c4dff" stroke-width="2" fill="none"/>
                        <line x1="20" y1="4" x2="36" y2="4" stroke="#7c4dff" stroke-width="2.5" stroke-linecap="round"/>
                        <path d="M22 20 L22 36 Q22 46 28 46 Q34 46 34 36 L34 20 Z" fill="#7c4dff" opacity="0.4"/>
                        <circle cx="26" cy="30" r="1.5" fill="#7c4dff" opacity="0.7"/>
                        <circle cx="30" cy="35" r="1" fill="#7c4dff" opacity="0.5"/>
                        <path d="M40 10 L40 36 Q40 46 46 46 Q52 46 52 36 L52 10" stroke="#7c4dff" stroke-width="2" fill="none"/>
                        <line x1="38" y1="10" x2="54" y2="10" stroke="#7c4dff" stroke-width="2.5" stroke-linecap="round"/>
                        <path d="M40 22 L40 36 Q40 46 46 46 Q52 46 52 36 L52 22 Z" fill="#7c4dff" opacity="0.4"/>
                      </svg>
                      <div class="brand-name">SETS</div>
                    </div>
                    <div class="brand-sub">Specimen Endorsement &amp; Tracking System</div>
                  </div>
                  <div class="badge"><span class="badge-dot"></span>Deploying Update</div>
                  <h1>System is Being Updated</h1>
                  <p class="subtitle">A new version of SETS is being deployed. This will only take a moment — the page will reload automatically.</p>
                  <div class="divider"></div>
                  <div class="progress-wrap"><div class="progress-bar"></div></div>
                  <p class="footer">Please do not close this tab.<br>You will be redirected automatically once the update is complete.</p>
                </div>
                </body>
                </html>
    """;


        public DeployService(DeployerSettings settings, Action<string, LogLevel> log)
        {
            _settings = settings;
            _log = log;
        }

        public async Task<bool> DeployToBranchAsync(BranchConfig branch, CancellationToken ct = default)
        {
            var uncPath = branch.UncPath;

            if (uncPath.Equals("NONE", StringComparison.OrdinalIgnoreCase) || !Directory.Exists(uncPath))
            {
                _log($"[{branch.Name}] UNC path not reachable: {uncPath}", LogLevel.Error);
                return false;
            }

            var offlineFile = Path.Combine(uncPath, APP_OFFLINE);

            try
            {
                // Step 1: Take site offline
                _log($"[{branch.Name}] Taking site offline (app_offline.htm)...", LogLevel.Info);
                await File.WriteAllTextAsync(offlineFile, APP_OFFLINE_CONTENT, ct);
                await Task.Delay(1500, ct);

                // Step 2: Copy published files
                _log($"[{branch.Name}] Copying files to {uncPath}...", LogLevel.Info);
                CopyPublishOutput(_settings.PublishOutputPath, uncPath, ct);

                // Step 3: Patch appsettings.json with correct branch name
                PatchAppSettings(uncPath, branch.Name);

                // Step 4: Bring site back online
                _log($"[{branch.Name}] Bringing site back online...", LogLevel.Info);
                if (File.Exists(offlineFile))
                    File.Delete(offlineFile);

                await Task.Delay(500, ct);
                _log($"[{branch.Name}] ✓ Deployment successful.", LogLevel.Success);
                return true;
            }
            catch (OperationCanceledException)
            {
                _log($"[{branch.Name}] Deployment cancelled.", LogLevel.Warning);
                TryRemoveOffline(offlineFile);
                return false;
            }
            catch (Exception ex)
            {
                _log($"[{branch.Name}] ✗ Deployment failed: {ex.Message}", LogLevel.Error);
                TryRemoveOffline(offlineFile);
                return false;
            }
        }

        // ── appsettings.json branch patch ─────────────────────────────────────

        private void PatchAppSettings(string deployPath, string branchName)
        {
            var appSettingsPath = Path.Combine(deployPath, "appsettings.json");
            if (!File.Exists(appSettingsPath))
            {
                _log($"  appsettings.json not found at destination — skipping branch patch.", LogLevel.Warning);
                return;
            }

            try
            {
                var json = File.ReadAllText(appSettingsPath);
                var node = JsonNode.Parse(json)!;

                // Ensure Default section exists then set Branch
                node["Default"] ??= new JsonObject();
                node["Default"]!["Branch"] = branchName;

                // Patch AppVersion if set in deployer settings
                if (!string.IsNullOrWhiteSpace(_settings.AppVersion))
                    node["AppVersion"] = _settings.AppVersion;

                var patched = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(appSettingsPath, patched);

                _log($"  appsettings.json patched — Default:Branch = \"{branchName}\"", LogLevel.Success);
            }
            catch (Exception ex)
            {
                _log($"  Failed to patch appsettings.json: {ex.Message}", LogLevel.Warning);
                // Non-fatal — deployment still succeeded
            }
        }

        // ── File copy ─────────────────────────────────────────────────────────

        private void CopyPublishOutput(string source, string dest, CancellationToken ct)
        {
            var allFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
            int total = allFiles.Length;
            int copied = 0;

            foreach (var file in allFiles)
            {
                ct.ThrowIfCancellationRequested();

                var rel    = Path.GetRelativePath(source, file);
                var target = Path.Combine(dest, rel);

                // Skip app_offline.htm we placed — don't overwrite it mid-copy
                if (Path.GetFileName(target).Equals(APP_OFFLINE, StringComparison.OrdinalIgnoreCase))
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, overwrite: true);
                copied++;

                if (copied % 50 == 0)
                    _log($"  {copied}/{total} files copied...", LogLevel.Dim);
            }

            _log($"  {copied} files copied.", LogLevel.Dim);
        }

        private static void TryRemoveOffline(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        // ── Connection test ───────────────────────────────────────────────────

        public static ConnectionStatus TestConnection(BranchConfig branch)
        {
            try
            {
                if (branch.UncPath.Equals("NONE", StringComparison.OrdinalIgnoreCase))
                    return ConnectionStatus.Unreachable;
                return Directory.Exists(branch.UncPath)
                    ? ConnectionStatus.Reachable
                    : ConnectionStatus.Unreachable;
            }
            catch
            {
                return ConnectionStatus.Unreachable;
            }
        }
    }

    public enum ConnectionStatus { Reachable, Unreachable, Unknown }
}
