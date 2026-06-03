using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using SETSDeployer.Config;
using SETSDeployer.Models;
using SETSDeployer.Services;
using SETSDeployer.Models;

namespace SETSDeployer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ── Collections ───────────────────────────────────────────────────
        public ObservableCollection<BranchViewModel> Branches { get; } = new();

        // ── Settings ──────────────────────────────────────────────────────
        private DeployerSettings _settings;
        public DeployerSettings Settings
        {
            get => _settings;
            set { _settings = value; OnPropertyChanged(); OnPropertyChanged(nameof(SolutionRootPath)); OnPropertyChanged(nameof(PublishOutputPath)); }
        }

        public string SolutionRootPath
        {
            get => _settings.SolutionRootPath;
            set { _settings.SolutionRootPath = value; OnPropertyChanged(); }
        }

        public string PublishOutputPath
        {
            get => _settings.PublishOutputPath;
            set { _settings.PublishOutputPath = value; OnPropertyChanged(); }
        }

        public string DeployerKey
        {
            get => _settings.DeployerKey;
            set { _settings.DeployerKey = value; OnPropertyChanged(); }
        }

        public string AppVersion
        {
            get => _settings.AppVersion;
            set { _settings.AppVersion = value; OnPropertyChanged(); }
        }

        // ── Build toggles ─────────────────────────────────────────────────
        private bool _buildFrontend = true;
        public bool BuildFrontend
        {
            get => _buildFrontend;
            set { _buildFrontend = value; OnPropertyChanged(); }
        }

        private bool _buildBackend = true;
        public bool BuildBackend
        {
            get => _buildBackend;
            set { _buildBackend = value; OnPropertyChanged(); }
        }

        // ── Deploy state ──────────────────────────────────────────────────
        private bool _isDeploying = false;
        public bool IsDeploying
        {
            get => _isDeploying;
            set { _isDeploying = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotDeploying)); }
        }
        public bool IsNotDeploying => !_isDeploying;

        private int _progress = 0;
        public int Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        // ── Add-branch form ───────────────────────────────────────────────
        private string _newName = "";
        public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }

        private string _newPath = "";
        public string NewPath { get => _newPath; set { _newPath = value; OnPropertyChanged(); } }

        private string _newPool = "SETS";
        public string NewPool { get => _newPool; set { _newPool = value; OnPropertyChanged(); } }

        private string _newMiddlewareUrl = "";
        public string NewMiddlewareUrl { get => _newMiddlewareUrl; set { _newMiddlewareUrl = value; OnPropertyChanged(); } }

        // ── Log action (set by view) ──────────────────────────────────────
        public Action<string, LogLevel>? LogAction { get; set; }

        private CancellationTokenSource? _cts;

        public MainViewModel()
        {
            _settings = ConfigManager.LoadSettings();
            var branches = ConfigManager.LoadBranches();
            foreach (var b in branches)
                Branches.Add(new BranchViewModel(b));
        }

        // ── Commands ──────────────────────────────────────────────────────

        public void SelectAll()   => Branches.ToList().ForEach(b => b.IsChecked = true);
        public void ClearAll()    => Branches.ToList().ForEach(b => b.IsChecked = false);

        public async Task DeployAsync()
        {
            if (IsDeploying) return;

            var selected = Branches.Where(b => b.IsChecked).ToList();
            if (selected.Count == 0) { Log("No branches selected.", LogLevel.Warning); return; }
            if (!BuildFrontend && !BuildBackend) { Log("Nothing selected to build.", LogLevel.Warning); return; }

            IsDeploying = true;
            _cts = new CancellationTokenSource();
            Progress = 0;

            int totalSteps = (BuildFrontend ? 2 : 0) + (BuildBackend ? 1 : 0) + selected.Count;
            int step = 0;
            void Advance(string msg) { step++; Progress = (int)(step * 100.0 / totalSteps); StatusText = msg; }

            var buildSvc  = new BuildService(_settings, Log);
            var deploySvc = new DeployService(_settings, Log);

            try
            {
                if (BuildFrontend)
                {
                    Log("━━━ Building Frontend ━━━", LogLevel.Info);
                    if (!await buildSvc.BuildFrontendAsync(_cts.Token))
                    { Log("Frontend build failed. Aborting.", LogLevel.Error); return; }
                    Advance("Frontend built.");
                }

                if (BuildBackend)
                {
                    Log("━━━ Publishing Backend ━━━", LogLevel.Info);
                    if (!await buildSvc.PublishBackendAsync(_cts.Token))
                    { Log("Backend publish failed. Aborting.", LogLevel.Error); return; }
                    Advance("Backend published.");
                }

                Log($"━━━ Deploying to {selected.Count} branch(es) simultaneously ━━━", LogLevel.Info);

                selected.ForEach(b => b.DeployStatus = "Deploying");

                var tasks = selected.Select(async b =>
                {
                    bool ok = await deploySvc.DeployToBranchAsync(b.Config, _cts.Token);
                    b.DeployStatus = ok ? "Success" : "Failed";
                    Advance(ok ? $"[{b.Name}] deployed." : $"[{b.Name}] failed.");
                    return ok;
                });

                var results = await Task.WhenAll(tasks);
                int ok2 = results.Count(r => r), fail = results.Length - ok2;
                Progress = 100;
                StatusText = $"Done — {ok2} succeeded, {fail} failed.";
                Log($"━━━ Complete: {ok2}/{selected.Count} succeeded ━━━",
                    fail == 0 ? LogLevel.Success : LogLevel.Warning);
            }
            catch (OperationCanceledException)
            {
                Log("Deployment cancelled.", LogLevel.Warning);
                StatusText = "Cancelled.";
                selected.Where(b => b.DeployStatus == "Deploying").ToList()
                    .ForEach(b => b.DeployStatus = "Idle");
            }
            catch (Exception ex)
            {
                Log($"Unexpected error: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                IsDeploying = false;
                _cts?.Dispose(); _cts = null;
            }
        }

        public void CancelDeploy() => _cts?.Cancel();

        public void AddBranch()
        {
            var name = NewName.Trim().ToUpperInvariant();
            var path = NewPath.Trim();
            var pool = string.IsNullOrWhiteSpace(NewPool) ? "SETS" : NewPool.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
            { System.Windows.MessageBox.Show("Branch name and UNC path are required.", "Validation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning); return; }

            if (Branches.Any(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            { System.Windows.MessageBox.Show($"Branch '{name}' already exists.", "Duplicate", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning); return; }

            var config = new BranchConfig { Name = name, UncPath = path, SiteName = pool, AppPoolName = pool, MiddlewareUrl = NewMiddlewareUrl.Trim() };
            Branches.Add(new BranchViewModel(config));
            SaveBranches();
            NewName = NewPath = NewMiddlewareUrl = "";
            NewPool = "SETS";
            Log($"Branch '{name}' added.", LogLevel.Success);
        }

        public void RemoveBranch(BranchViewModel branch)
        {
            var result = System.Windows.MessageBox.Show($"Remove branch '{branch.Name}'?", "Confirm",
                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                Branches.Remove(branch);
                SaveBranches();
                Log($"Branch '{branch.Name}' removed.", LogLevel.Warning);
            }
        }

        public async Task TestAllConnectionsAsync()
        {
            Log("Testing all branch connections...", LogLevel.Info);
            foreach (var b in Branches)
            {
                var status = await Task.Run(() => DeployService.TestConnection(b.Config));
                b.ConnStatus = status.ToString();
                Log(status == ConnectionStatus.Reachable
                    ? $"[{b.Name}] ✓ Reachable"
                    : $"[{b.Name}] ✗ Unreachable",
                    status == ConnectionStatus.Reachable ? LogLevel.Success : LogLevel.Error);
            }
        }

        public async Task IisActionAsync(BranchViewModel branch, string action)
        {
            var iis = new IisService(Log);
            bool ok = action switch
            {
                "Start"   => await iis.StartSiteAsync(branch.Config),
                "Stop"    => await iis.StopSiteAsync(branch.Config),
                "Restart" => await iis.RestartSiteAsync(branch.Config),
                _         => false
            };
            if (!ok) Log($"[{branch.Name}] IIS {action} did not complete.", LogLevel.Warning);
        }

        public void SaveSettings()
        {
            ConfigManager.SaveSettings(_settings);
            Log("Settings saved.", LogLevel.Success);
            System.Windows.MessageBox.Show("Settings saved.", "SETS Deployer", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        public async Task EnableMaintenanceAsync(IEnumerable<BranchViewModel> targets)
        {
            var svc = new MaintenanceService(Log);
            foreach (var b in targets)
                await svc.EnableAsync(b.Config);
        }

        public async Task DisableMaintenanceAsync(IEnumerable<BranchViewModel> targets)
        {
            var svc = new MaintenanceService(Log);
            foreach (var b in targets)
                await svc.DisableAsync(b.Config);
        }

        // ── SQL Runner ────────────────────────────────────────────────────────

        private string _sqlQuery = string.Empty;
        public string SqlQuery
        {
            get => _sqlQuery;
            set { _sqlQuery = value; OnPropertyChanged(); }
        }

        private bool _isSqlRunning = false;
        public bool IsSqlRunning
        {
            get => _isSqlRunning;
            set { _isSqlRunning = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSqlNotRunning)); }
        }
        public bool IsSqlNotRunning => !_isSqlRunning;

        private CancellationTokenSource? _sqlCts;

        public ObservableCollection<SqlRunnerResult> SqlResults { get; } = new();

        public async Task RunSqlAsync(IEnumerable<BranchViewModel> targets)
        {
            var svc = new SqlRunnerService(_settings, Log);

            // Validate
            var (valid, reason) = svc.ValidateQuery(SqlQuery);
            if (!valid) { Log($"Validation failed: {reason}", LogLevel.Error); return; }

            // Load connection strings
            Dictionary<string, string> connStrings;
            try
            {
                connStrings = svc.LoadConnectionStrings();
            }
            catch (Exception ex)
            {
                Log($"Could not load connection strings: {ex.Message}", LogLevel.Error);
                return;
            }

            var selectedBranches = targets.Select(b => b.Config).ToList();
            if (!selectedBranches.Any()) { Log("No branches selected.", LogLevel.Warning); return; }

            IsSqlRunning = true;
            _sqlCts = new CancellationTokenSource();
            SqlResults.Clear();

            Log($"━━━ Executing on {selectedBranches.Count} branch(es) ━━━", LogLevel.Info);
            Log(SqlQuery.Trim(), LogLevel.Dim);

            try
            {
                var results = await svc.ExecuteAsync(SqlQuery, selectedBranches, connStrings, _sqlCts.Token);
                foreach (var r in results)
                    System.Windows.Application.Current.Dispatcher.Invoke(() => SqlResults.Add(r));
            }
            finally
            {
                IsSqlRunning = false;
                _sqlCts?.Dispose();
                _sqlCts = null;
            }
        }

        public void CancelSql() => _sqlCts?.Cancel();

        public void SaveBranches()
        {
            ConfigManager.SaveBranches(Branches.Select(b => b.Config).ToList());
        }

        private void Log(string msg, LogLevel level = LogLevel.Dim)
            => LogAction?.Invoke(msg, level);

        // ── INotifyPropertyChanged ─────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
