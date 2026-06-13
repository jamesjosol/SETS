using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using SETSDeployer.Services;
using SETSDeployer.ViewModels;

namespace SETSDeployer.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        private static readonly SolidColorBrush _brushSuccess =
            new(System.Windows.Media.Color.FromRgb(46, 204, 113));
        private static readonly SolidColorBrush _brushDanger =
            new(System.Windows.Media.Color.FromRgb(239, 68, 68));
        private static readonly SolidColorBrush _brushWarning =
            new(System.Windows.Media.Color.FromRgb(245, 158, 11));
        private static readonly SolidColorBrush _brushInfo =
            new(System.Windows.Media.Color.FromRgb(79, 142, 247));
        private static readonly SolidColorBrush _brushDim =
            new(System.Windows.Media.Color.FromRgb(139, 149, 170));
        private static readonly SolidColorBrush _brushTs =
            new(System.Windows.Media.Color.FromRgb(90, 100, 120));

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            _vm.LogAction = AppendLog;
            DataContext = _vm;
        }

        // ── Logging ───────────────────────────────────────────────────────

        private void AppendLog(string message, LogLevel level)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => AppendLog(message, level));
                return;
            }

            var doc = LogBox.Document;
            var para = new Paragraph { Margin = new Thickness(0) };

            var tsBrush = _brushTs;
            var msgBrush = level switch
            {
                LogLevel.Success => _brushSuccess,
                LogLevel.Error   => _brushDanger,
                LogLevel.Warning => _brushWarning,
                LogLevel.Info    => _brushInfo,
                _                => _brushDim
            };

            para.Inlines.Add(new Run($"[{DateTime.Now:HH:mm:ss}] ") { Foreground = tsBrush });
            para.Inlines.Add(new Run(message) { Foreground = msgBrush });
            doc.Blocks.Add(para);
            LogBox.ScrollToEnd();
        }

        // ── Deploy tab events ─────────────────────────────────────────────

        private void FeOption_Click(object sender, RoutedEventArgs e)
            => _vm.BuildFrontend = !_vm.BuildFrontend;

        private void BeOption_Click(object sender, RoutedEventArgs e)
            => _vm.BuildBackend = !_vm.BuildBackend;

        private void SelectAll_Click(object sender, RoutedEventArgs e)  => _vm.SelectAll();
        private void ClearAll_Click(object sender, RoutedEventArgs e)   => _vm.ClearAll();

        private void BranchRow_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is BranchViewModel bvm)
                bvm.IsChecked = !bvm.IsChecked;
        }

        private async void Deploy_Click(object sender, RoutedEventArgs e)
            => await _vm.DeployAsync();

        private void Cancel_Click(object sender, RoutedEventArgs e)
            => _vm.CancelDeploy();

        private void ClearLog_Click(object sender, RoutedEventArgs e)
            => LogBox.Document.Blocks.Clear();

        // ── Manage Branches tab events ────────────────────────────────────

        private async void TestAll_Click(object sender, RoutedEventArgs e)
            => await _vm.TestAllConnectionsAsync();

        private async void IisStop_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is BranchViewModel b)
                await _vm.IisActionAsync(b, "Stop");
        }

        private async void IisStart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is BranchViewModel b)
                await _vm.IisActionAsync(b, "Start");
        }

        private async void IisRestart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is BranchViewModel b)
                await _vm.IisActionAsync(b, "Restart");
        }

        private void RemoveBranch_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is BranchViewModel b)
                _vm.RemoveBranch(b);
        }

        private void AddBranch_Click(object sender, RoutedEventArgs e)
            => _vm.AddBranch();

        // ── Maintenance ───────────────────────────────────────────────────────

        private void MaintRow_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is BranchViewModel b)
                b.IsMaintChecked = !b.IsMaintChecked;
        }

        private async void EnableMaintenance_Click(object sender, RoutedEventArgs e)
        {
            var targets = _vm.Branches.Where(b => b.IsMaintChecked).ToList();
            if (!targets.Any())
            {
                AppendLog("No branches selected for maintenance.", LogLevel.Warning);
                return;
            }

            var names = string.Join(", ", targets.Select(b => b.Name));
            var result = System.Windows.MessageBox.Show(
                $"Enable maintenance mode on: {names}? Users will see the maintenance page immediately.",
                "Enable Maintenance",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes) return;

            await _vm.EnableMaintenanceAsync(targets);
        }

        private async void DisableMaintenance_Click(object sender, RoutedEventArgs e)
        {
            var targets = _vm.Branches.Where(b => b.IsMaintChecked).ToList();
            if (!targets.Any())
            {
                AppendLog("No branches selected.", LogLevel.Warning);
                return;
            }

            await _vm.DisableMaintenanceAsync(targets);
        }


        // ── Settings tab events ───────────────────────────────────────────

        // ── SQL Runner ────────────────────────────────────────────────────────

        private void ClearSql_Click(object sender, RoutedEventArgs e)
            => _vm.SqlQuery = string.Empty;

        private void SqlSelectAll_Click(object sender, RoutedEventArgs e)
            => _vm.Branches.ToList().ForEach(b => b.IsSqlChecked = true);

        private void SqlClearAll_Click(object sender, RoutedEventArgs e)
            => _vm.Branches.ToList().ForEach(b => b.IsSqlChecked = false);

        private void SqlBranchRow_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is BranchViewModel b)
                b.IsSqlChecked = !b.IsSqlChecked;
        }

        private async void RunSql_Click(object sender, RoutedEventArgs e)
        {
            var targets = _vm.Branches.Where(b => b.IsSqlChecked).ToList();
            if (!targets.Any()) { AppendLog("No branches selected.", LogLevel.Warning); return; }

            var names = string.Join(", ", targets.Select(b => b.Name));
            var confirm = System.Windows.MessageBox.Show(
                $"Execute DDL on: {names}? This cannot be undone.",
                "Confirm SQL Execution",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (confirm != System.Windows.MessageBoxResult.Yes) return;

            await _vm.RunSqlAsync(targets);
        }

        private void CancelSql_Click(object sender, RoutedEventArgs e)
            => _vm.CancelSql();


        // ── SQL Runner (DML) ──────────────────────────────────────────────────

        private void ClearDml_Click(object sender, RoutedEventArgs e)
            => _vm.DmlQuery = string.Empty;

        private void DmlSelectAll_Click(object sender, RoutedEventArgs e)
            => _vm.Branches.ToList().ForEach(b => b.IsDmlChecked = true);

        private void DmlClearAll_Click(object sender, RoutedEventArgs e)
            => _vm.Branches.ToList().ForEach(b => b.IsDmlChecked = false);

        private void DmlBranchRow_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is BranchViewModel b)
                b.IsDmlChecked = !b.IsDmlChecked;
        }

        private async void RunDml_Click(object sender, RoutedEventArgs e)
        {
            var targets = _vm.Branches.Where(b => b.IsDmlChecked).ToList();
            if (!targets.Any()) { AppendLog("No branches selected.", LogLevel.Warning); return; }

            var names = string.Join(", ", targets.Select(b => b.Name));
            var confirm = System.Windows.MessageBox.Show(
                $"Execute DML on: {names}?\n\nThis will modify data and cannot be undone.",
                "Confirm DML Execution",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (confirm != System.Windows.MessageBoxResult.Yes) return;

            await _vm.RunDmlAsync(targets);
        }

        private void CancelDml_Click(object sender, RoutedEventArgs e)
            => _vm.CancelDml();

        private void BrowseSolution_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = _vm.SolutionRootPath
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _vm.SolutionRootPath = dlg.SelectedPath;
        }

        private void BrowsePublish_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = _vm.PublishOutputPath
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _vm.PublishOutputPath = dlg.SelectedPath;
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
            => _vm.SaveSettings();
    }
}
