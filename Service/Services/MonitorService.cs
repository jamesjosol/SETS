using System;
using System.Net.Http;
using System.Threading.Tasks;
using Service.Interfaces;

namespace Service.Services
{
    /// <summary>
    /// Server-side proxy to the local SETSMiddleware monitor endpoint.
    ///
    /// SETSMiddleware runs on the same box as IIS/SETS and samples the SETS
    /// app pool's w3wp process (CPU/RAM/IO) plus the IIS per-site network
    /// counters. This service calls GET {baseUrl}/monitor/sets with the
    /// X-Deployer-Key header so the key never reaches the browser.
    ///
    /// No database access — purely an HTTP passthrough. Registered in
    /// MasterService for consistency with the Controller → MasterService rule.
    /// </summary>
    public class MonitorService : IMonitorService
    {
        // Short timeout — the middleware is localhost; if it doesn't answer
        // fast, it's down, and the dashboard should show "offline" quickly.
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private readonly string _branch;

        public MonitorService(string branch)
        {
            _branch = branch;
        }

        public async Task<(int StatusCode, string Json)> GetSetsSnapshotAsync(
            string baseUrl, string deployerKey, bool includeRequests = true)
        {
            var url = $"{baseUrl.TrimEnd('/')}/monitor/sets"
                    + (includeRequests ? string.Empty : "?requests=0");

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("X-Deployer-Key", deployerKey);

            var resp = await _http.SendAsync(req);
            var json = await resp.Content.ReadAsStringAsync();

            return ((int)resp.StatusCode, json);
        }
    }
}