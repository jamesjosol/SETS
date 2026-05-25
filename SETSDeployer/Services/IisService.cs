using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using SETSDeployer.Models;

namespace SETSDeployer.Services
{
    /// <summary>
    /// Controls IIS on remote branch servers by calling the SETSMiddleware HTTP server
    /// running on the same machine as IIS (http://BRANCH_IP:5100).
    /// The middleware executes appcmd locally and returns the result.
    /// Secured by X-Deployer-Key header matching the key in branches.json.
    /// </summary>
    public class IisService
    {
        private readonly Action<string, LogLevel> _log;
        private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

        public IisService(Action<string, LogLevel> log)
        {
            _log = log;
        }

        public async Task<bool> StartSiteAsync(BranchConfig branch)
        {
            _log($"[{branch.Name}] Starting site '{branch.SiteName}'...", LogLevel.Info);
            return await SendIisCommandAsync(branch, "start");
        }

        public async Task<bool> StopSiteAsync(BranchConfig branch)
        {
            _log($"[{branch.Name}] Stopping site '{branch.SiteName}'...", LogLevel.Info);
            return await SendIisCommandAsync(branch, "stop");
        }

        public async Task<bool> RestartSiteAsync(BranchConfig branch)
        {
            _log($"[{branch.Name}] Restarting site '{branch.SiteName}'...", LogLevel.Info);
            return await SendIisCommandAsync(branch, "restart");
        }

        private async Task<bool> SendIisCommandAsync(BranchConfig branch, string action)
        {
            if (string.IsNullOrWhiteSpace(branch.MiddlewareUrl))
            {
                _log($"[{branch.Name}] MiddlewareUrl is not configured for this branch.", LogLevel.Error);
                return false;
            }

            var url = $"{branch.MiddlewareUrl.TrimEnd('/')}/iis/{action}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("X-Deployer-Key", DeployerKeyProvider.Key);

                var response = await _http.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _log($"[{branch.Name}] ✓ IIS {action} succeeded.", LogLevel.Success);
                    return true;
                }

                // Try parse error message from JSON body
                string errorMsg = body;
                try
                {
                    var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("message", out var msg))
                        errorMsg = msg.GetString() ?? body;
                }
                catch { }

                _log($"[{branch.Name}] IIS {action} failed ({(int)response.StatusCode}): {errorMsg}", LogLevel.Error);
                return false;
            }
            catch (TaskCanceledException)
            {
                _log($"[{branch.Name}] IIS {action} timed out — is middleware running at {branch.MiddlewareUrl}?", LogLevel.Error);
                return false;
            }
            catch (Exception ex)
            {
                _log($"[{branch.Name}] IIS {action} error: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
    }

    /// <summary>
    /// Holds the shared deployer key loaded from deployer.json.
    /// Set once at startup by ConfigManager.
    /// </summary>
    public static class DeployerKeyProvider
    {
        public static string Key { get; set; } = string.Empty;
    }
}
