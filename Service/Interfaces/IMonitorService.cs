using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IMonitorService
    {
        /// <summary>
        /// Fetches the SETS-only resource snapshot from the local SETSMiddleware
        /// (/monitor/sets). Returns the middleware's HTTP status code and raw JSON
        /// body — the controller passes both through untouched.
        /// </summary>
        Task<(int StatusCode, string Json)> GetSetsSnapshotAsync(
            string baseUrl, string deployerKey, bool includeRequests = true);
    }
}