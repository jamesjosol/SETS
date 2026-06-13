using System.ComponentModel;
using System.Runtime.CompilerServices;
using SETSDeployer.Models;

namespace SETSDeployer.ViewModels
{
    public class BranchViewModel : INotifyPropertyChanged
    {
        public BranchConfig Config { get; }

        public BranchViewModel(BranchConfig config)
        {
            Config = config;
        }

        // ── Forwarded properties ──────────────────────────────────────────

        public string Name
        {
            get => Config.Name;
            set { Config.Name = value; OnPropertyChanged(); }
        }

        public string UncPath
        {
            get => Config.UncPath;
            set { Config.UncPath = value; OnPropertyChanged(); }
        }

        public string SiteName
        {
            get => Config.SiteName;
            set { Config.SiteName = value; OnPropertyChanged(); }
        }

        public string AppPoolName
        {
            get => Config.AppPoolName;
            set { Config.AppPoolName = value; OnPropertyChanged(); }
        }

        public string MiddlewareUrl
        {
            get => Config.MiddlewareUrl;
            set { Config.MiddlewareUrl = value; OnPropertyChanged(); }
        }

        // ── UI-only state ─────────────────────────────────────────────────

        private bool _isChecked = true;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(); }
        }

        private string _deployStatus = "Idle";
        public string DeployStatus
        {
            get => _deployStatus;
            set { _deployStatus = value; OnPropertyChanged(); }
        }

        private string _connStatus = "Unknown";
        public string ConnStatus
        {
            get => _connStatus;
            set { _connStatus = value; OnPropertyChanged(); }
        }

        private bool _isMaintChecked = false;
        public bool IsMaintChecked
        {
            get => _isMaintChecked;
            set { _isMaintChecked = value; OnPropertyChanged(); }
        }

        private bool _isSqlChecked = false;
        public bool IsSqlChecked
        {
            get => _isSqlChecked;
            set { _isSqlChecked = value; OnPropertyChanged(); }
        }

        private bool _isDmlChecked = false;
        public bool IsDmlChecked
        {
            get => _isDmlChecked;
            set { _isDmlChecked = value; OnPropertyChanged(); }
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}