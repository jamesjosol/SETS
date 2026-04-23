using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using SETSMiddleware.Tasks;


namespace SETSMiddleware
{
    /// <summary>
    /// Lightweight self-hosted HTTP server that exposes GET http://localhost:5100/health
    /// so the SETS web app can check if the middleware is alive and what tasks are running.
    /// </summary>
    public class MiddlewareHttpServer
    {
        private readonly string _branch;
        private readonly List<TaskBase> _tasks;
        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private Task _listenTask;

        public bool IsRunning { get; private set; }

        public MiddlewareHttpServer(string branch, List<TaskBase> tasks)
        {
            _branch = branch;
            _tasks = tasks;
        }

        public void Start()
        {
            if (IsRunning) return;

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:5100/");
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
                catch (HttpListenerException)
                {
                    // Listener was stopped — normal shutdown
                    break;
                }
                catch (Exception)
                {
                    // Ignore other transient errors
                }
            }
        }

        private void HandleRequest(HttpListenerContext ctx)
        {
            try
            {
                var req = ctx.Request;
                var resp = ctx.Response;

                // Only handle GET /health
                if (req.HttpMethod == "GET" &&
                    req.Url.AbsolutePath.TrimEnd('/').Equals("/health", StringComparison.OrdinalIgnoreCase))
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

                    var json = JsonSerializer.Serialize(payload);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    resp.StatusCode = 200;
                    resp.ContentType = "application/json";
                    resp.AddHeader("Access-Control-Allow-Origin", "*");
                    resp.ContentLength64 = bytes.Length;
                    resp.OutputStream.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    resp.StatusCode = 404;
                }

                resp.OutputStream.Close();
            }
            catch { /* ignore */ }
        }
    }
}