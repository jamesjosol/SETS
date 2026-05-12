using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Reposi;

namespace SETSMiddleware
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // ── Load appsettings.json (shared with SETS.Server) ──────────────────
            // The middleware sits in the same solution, so we point to the
            // server project's appsettings.json to keep connection strings in
            // one place. Adjust the relative path if your output directory differs.
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            // ── Initialize SetsConnection so MasterService can resolve branch ────
            SetsConnection.Initialize(config);

            // ── Branch for this instance ─────────────────────────────────────────
            // Change this to match the branch this deployment serves.
            // Each branch runs its own instance of SETSMiddleware.
            const string BRANCH = "DIAMOND";

            Application.Run(new MiddlewareForm(BRANCH, config));
        }
    }
}