using System.IO;
using System.Text.Json;
using SETSDeployer.Models;
using SETSDeployer.Services;

namespace SETSDeployer.Config
{
    public static class ConfigManager
    {
        private static readonly string _appDir = AppContext.BaseDirectory;
        private static readonly string _branchesPath = Path.Combine(_appDir, "branches.json");
        private static readonly string _settingsPath = Path.Combine(_appDir, "deployer.json");

        private static readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        // ── Branches ──────────────────────────────────────────────────────────

        public static List<BranchConfig> LoadBranches()
        {
            if (!File.Exists(_branchesPath))
            {
                var defaults = GetDefaultBranches();
                SaveBranches(defaults);
                return defaults;
            }

            var json = File.ReadAllText(_branchesPath);
            return JsonSerializer.Deserialize<List<BranchConfig>>(json, _opts) ?? GetDefaultBranches();
        }

        public static void SaveBranches(List<BranchConfig> branches)
        {
            File.WriteAllText(_branchesPath, JsonSerializer.Serialize(branches, _opts));
        }

        private static List<BranchConfig> GetDefaultBranches()
        {
            var names = new[] { "WES", "DIAMOND", "TABUNOK", "MACTAN", "NAGA", "LILOAN", "CONSOLACION" };
            return names.Select(n => new BranchConfig
            {
                Name          = n,
                UncPath       = $@"\\APPSERVER-{n}\sets-iis",
                SiteName      = "SETS",
                AppPoolName   = "SETS",
                MiddlewareUrl = $"http://APPSERVER-{n}:5100",
                IsEnabled     = true
            }).ToList();
        }

        // ── Settings ──────────────────────────────────────────────────────────

        public static DeployerSettings LoadSettings()
        {
            DeployerSettings settings;

            if (!File.Exists(_settingsPath))
            {
                settings = new DeployerSettings
                {
                    SolutionRootPath  = @"C:\Projects\SETS",
                    PublishOutputPath = @"C:\Projects\SETS\publish",
                    DeployerKey       = "sets-deployer-2024"
                };
                SaveSettings(settings);
            }
            else
            {
                var json = File.ReadAllText(_settingsPath);
                settings = JsonSerializer.Deserialize<DeployerSettings>(json, _opts) ?? new DeployerSettings();
            }

            // Make the key available globally to IisService
            DeployerKeyProvider.Key = settings.DeployerKey;
            return settings;
        }

        public static void SaveSettings(DeployerSettings settings)
        {
            DeployerKeyProvider.Key = settings.DeployerKey;
            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, _opts));
        }
    }
}
