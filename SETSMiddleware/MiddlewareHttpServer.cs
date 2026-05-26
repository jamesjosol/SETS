using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using SETSMiddleware.Tasks;

namespace SETSMiddleware
{
    /// <summary>
    /// Lightweight self-hosted HTTP server.
    ///
    /// Endpoints:
    ///   GET  /health          — health check (called by SETS web app on localhost)
    ///   POST /iis/stop        — stop the SETS IIS site (called by SETSDeployer over LAN)
    ///   POST /iis/start       — start the SETS IIS site
    ///   POST /iis/restart     — restart the SETS IIS site
    ///
    /// IIS endpoints require X-Deployer-Key header matching DeployerKey in appsettings.json.
    /// Listener binds to *:5100 so both localhost and LAN calls are accepted.
    /// </summary>
    public class MiddlewareHttpServer
    {
        private readonly string _branch;
        private readonly List<TaskBase> _tasks;
        private readonly string _deployerKey;
        private readonly string _siteName;

        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private Task _listenTask;

        public bool IsRunning { get; private set; }

        public MiddlewareHttpServer(string branch, List<TaskBase> tasks, string deployerKey, string siteName = "SETS")
        {
            _branch = branch;
            _tasks = tasks;
            _deployerKey = deployerKey;
            _siteName = siteName;
        }

        public void Start()
        {
            if (IsRunning) return;

            _listener = new HttpListener();
            // Bind on all interfaces so SETSDeployer can reach it over the LAN.
            // localhost calls from the SETS web app (/health) still work fine.
            _listener.Prefixes.Add("http://localhost:5100/"); // _listener.Prefixes.Add("http://*:5100/");
            _listener.Start();

            _cts = new CancellationTokenSource();
            IsRunning = true;

            _listenTask = Task.Run(() => ListenAsync(_cts.Token));
        }

        public void Stop()
        {
            if (!IsRunning) return;
            _cts?.Cancel();
            _listener?.Stop();
            IsRunning = false;
        }

        private async Task ListenAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var ctx = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(ctx));
                }
                catch (HttpListenerException) { break; }
                catch { }
            }
        }

        private void HandleRequest(HttpListenerContext ctx)
        {
            try
            {
                var req = ctx.Request;
                var resp = ctx.Response;
                var path = req.Url.AbsolutePath.TrimEnd('/').ToLowerInvariant();

                resp.AddHeader("Access-Control-Allow-Origin", "*");

                // ── GET /health ───────────────────────────────────────────────
                if (req.HttpMethod == "GET" && path == "/health")
                {
                    var payload = new
                    {
                        online = true,
                        branch = _branch,
                        tasks = _tasks.Select(t => new
                        {
                            name = t.TaskName,
                            running = t.IsRunning,
                            lastRun = t.LastRun?.ToString("HH:mm:ss") ?? "Never",
                            status = t.LastStatus
                        })
                    };
                    WriteJson(resp, 200, payload);
                    return;
                }

                // ── POST /iis/* ───────────────────────────────────────────────
                if (req.HttpMethod == "POST" && path.StartsWith("/iis/"))
                {
                    // Validate key
                    var key = req.Headers["X-Deployer-Key"];
                    if (string.IsNullOrEmpty(_deployerKey) || key != _deployerKey)
                    {
                        WriteJson(resp, 401, new { message = "Unauthorized. Invalid or missing X-Deployer-Key." });
                        return;
                    }

                    var action = path.Replace("/iis/", ""); // stop | start | restart

                    bool ok = action switch
                    {
                        "stop" => RunAppcmd($"stop site /site.name:\"{_siteName}\""),
                        "start" => RunAppcmd($"start site /site.name:\"{_siteName}\""),
                        "restart" => RunRestart(),
                        _ => false
                    };

                    if (ok)
                        WriteJson(resp, 200, new { success = true, action, site = _siteName });
                    else
                        WriteJson(resp, 500, new { success = false, message = $"appcmd {action} failed. Check middleware logs." });

                    return;
                }

                // ── 404 ───────────────────────────────────────────────────────
                resp.StatusCode = 404;
                resp.OutputStream.Close();
            }
            catch { /* ignore */ }
        }

        // ── IIS helpers ───────────────────────────────────────────────────────

        private bool RunRestart()
        {
            RunAppcmd($"stop site /site.name:\"{_siteName}\"");
            System.Threading.Thread.Sleep(1200);
            return RunAppcmd($"start site /site.name:\"{_siteName}\"");
        }

        private static bool RunAppcmd(string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\inetsrv\appcmd.exe",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi)!;
                proc.WaitForExit(10_000);
                return proc.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        // ── Response helper ───────────────────────────────────────────────────

        private static void WriteJson(HttpListenerResponse resp, int statusCode, object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            resp.StatusCode = statusCode;
            resp.ContentType = "application/json";
            resp.ContentLength64 = bytes.Length;
            resp.OutputStream.Write(bytes, 0, bytes.Length);
            resp.OutputStream.Close();
        }
    }
}
