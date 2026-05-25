namespace SETSDeployer.Models
{
    public class BranchConfig
    {
        public string Name { get; set; } = string.Empty;
        public string UncPath { get; set; } = string.Empty;
        public string SiteName { get; set; } = "SETS";
        public string AppPoolName { get; set; } = "SETS";
        public string MiddlewareUrl { get; set; } = string.Empty; // e.g. http://192.171.10.74:5100
        public bool IsEnabled { get; set; } = true;
    }
}
