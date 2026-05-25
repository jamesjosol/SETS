namespace SETSDeployer.Models
{
    public class DeployerSettings
    {
        public string SolutionRootPath { get; set; } = string.Empty;
        public string PublishOutputPath { get; set; } = string.Empty;

        /// <summary>
        /// Shared secret sent as X-Deployer-Key header to SETSMiddleware IIS endpoints.
        /// Must match DeployerKey in each branch's appsettings.json.
        /// </summary>
        public string DeployerKey { get; set; } = "sets-deployer-2024";

        /// <summary>
        /// App version patched into each branch's appsettings.json on deploy.
        /// Displayed on the SETS login page footer.
        /// </summary>
        public string AppVersion { get; set; } = "1.0.0";
    }
}
