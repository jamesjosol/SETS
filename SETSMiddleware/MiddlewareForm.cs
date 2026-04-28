using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using SETSMiddleware.Tasks;

namespace SETSMiddleware
{
    public partial class MiddlewareForm : Form
    {
        // ── Colors matching SETS dark theme ──────────────────────────────────────
        public static readonly Color C_BG = ColorTranslator.FromHtml("#0f1117");
        public static readonly Color C_SURFACE = ColorTranslator.FromHtml("#1e2130");
        public static readonly Color C_SURFACE_LOW = ColorTranslator.FromHtml("#252838");
        public static readonly Color C_SURFACE_HIGH = ColorTranslator.FromHtml("#2a2d3e");
        public static readonly Color C_BORDER = ColorTranslator.FromHtml("#2e3147");
        public static readonly Color C_PRIMARY = ColorTranslator.FromHtml("#7c4dff");
        public static readonly Color C_PRIMARY_SOFT = ColorTranslator.FromHtml("#1e1535");
        public static readonly Color C_TEXT = ColorTranslator.FromHtml("#e8eaf0");
        public static readonly Color C_TEXT_MUTED = ColorTranslator.FromHtml("#8b8fa8");
        public static readonly Color C_SUCCESS = ColorTranslator.FromHtml("#059669");
        public static readonly Color C_WARNING = ColorTranslator.FromHtml("#d97706");
        public static readonly Color C_ERROR = ColorTranslator.FromHtml("#ef4444");

        // ── State ─────────────────────────────────────────────────────────────────
        private readonly string _branch;
        private readonly List<TaskBase> _tasks = new();
        private TaskBase _selectedTask;
        private TaskRow _selectedRow;
        private MiddlewareHttpServer _httpServer;
        private readonly IConfiguration _config;

        // ── Log buffer per task ───────────────────────────────────────────────────
        private readonly Dictionary<string, List<(string message, TaskBase.LogLevel level, DateTime time)>> _logs = new();

        public MiddlewareForm(string branch, IConfiguration config)
        {
            _branch = branch;
            _config = config;
            InitializeComponent();
            SetupTheme();
            RegisterTasks();
            BuildTaskList();

            // Start the health endpoint so the web app can detect this instance
            _httpServer = new MiddlewareHttpServer(_branch, _tasks);
            _httpServer.Start();

            if (_tasks.Count > 0)
                SelectTask(_taskRows[0], _tasks[0]);
        }

        // ── Task Registration ─────────────────────────────────────────────────────
        private void RegisterTasks()
        {
            int hclabInterval = int.TryParse(_config["TaskIntervals:HclabRouting"], out var v1) ? v1 : 30;
            int schedInterval = int.TryParse(_config["TaskIntervals:ScheduledSpecimenRelease"], out var v2) ? v2 : 120;
            int releaseInterval = int.TryParse(_config["TaskIntervals:TestResultRelease"], out var v3) ? v3 : 60;
            int syncInterval = int.TryParse(_config["TaskIntervals:HclabReferenceSync"], out var v4) ? v4 : 43200;
            int tatResetInterval = int.TryParse(_config["TaskIntervals:EndorsementTatReset"], out var v5) ? v5 : 60;

            var hclabTask = new HclabRoutingTask(_branch, hclabInterval);
            var schedTask = new ScheduledSpecimenReleaseTask(_branch, schedInterval);
            var releaseTask = new TestResultReleaseTask(_branch, releaseInterval);
            var syncTask = new HclabReferenceSyncTask(_branch, syncInterval);
            var tatResetTask = new EndorsementTatResetTask(_branch, tatResetInterval);

            foreach (var task in new TaskBase[] { hclabTask, schedTask, releaseTask, syncTask, tatResetTask })
            {
                _tasks.Add(task);
                _logs[task.TaskName] = new List<(string, TaskBase.LogLevel, DateTime)>();
            }

            foreach (var task in _tasks)
            {
                task.OnLog += (msg, level) => AppendLog(task, msg, level);
                task.OnStateChanged += () => RefreshUI(task);
            }
        }

        // ── Task List (left panel) ─────────────────────────────────────────────────

        private readonly List<TaskRow> _taskRows = new();

        private void BuildTaskList()
        {
            pnlTaskList.Controls.Clear();
            _taskRows.Clear();

            foreach (var task in _tasks)
            {
                var row = new TaskRow(task);
                row.Click += (s, e) => SelectTask(row, task);
                row.Cursor = Cursors.Hand;
                pnlTaskList.Controls.Add(row);
                _taskRows.Add(row);
            }
        }

        private void SelectTask(TaskRow row, TaskBase task)
        {
            // Deselect previous
            if (_selectedRow != null)
                _selectedRow.SetSelected(false);

            _selectedTask = task;
            _selectedRow = row;
            row.SetSelected(true);

            RefreshDetail(task);
        }

        // ── Detail Panel (right panel) ────────────────────────────────────────────

        private void RefreshDetail(TaskBase task)
        {
            if (task == null) return;

            lblTaskTitle.Text = task.TaskName;
            lblTaskDescription.Text = task.Description;
            lblInterval.Text = $"Interval: {task.IntervalSeconds}s";
            lblInterval.BringToFront();
            lblInterval.Refresh();
            lblLastRun.Text = task.LastRun.HasValue
                ? $"Last run: {task.LastRun.Value:HH:mm:ss}"
                : "Last run: Never";
            lblStatus.Text = task.LastStatus;

            UpdateActionButtons(task);
            RedrawLog(task);
        }

        private void UpdateActionButtons(TaskBase task)
        {
            if (task == null) return;

            if (task.IsRunning)
            {
                btnToggle.Text = "⏹  Stop Task";
                btnToggle.BackColor = ColorTranslator.FromHtml("#2d1515");
                btnToggle.ForeColor = C_ERROR;
            }
            else
            {
                btnToggle.Text = "▶  Start Task";
                btnToggle.BackColor = C_PRIMARY_SOFT;
                btnToggle.ForeColor = C_PRIMARY;
            }
        }

        // ── Button Handlers ───────────────────────────────────────────────────────

        private void btnToggle_Click(object sender, EventArgs e)
        {
            if (_selectedTask == null) return;

            if (_selectedTask.IsRunning)
                _selectedTask.Stop();
            else
                _selectedTask.Start();
        }

        private async void btnRunNow_Click(object sender, EventArgs e)
        {
            if (_selectedTask == null) return;
            btnRunNow.Enabled = false;
            btnRunNow.Text = "Running...";
            try { await _selectedTask.RunNowAsync(); }
            finally
            {
                btnRunNow.Enabled = true;
                btnRunNow.Text = "⚡  Run Now";
            }
        }

        // ── Log ───────────────────────────────────────────────────────────────────

        private void AppendLog(TaskBase task, string message, TaskBase.LogLevel level)
        {
            if (InvokeRequired)
            {
                Invoke(() => AppendLog(task, message, level));
                return;
            }

            var buffer = _logs[task.TaskName];
            buffer.Add((message, level, DateTime.Now));

            // Keep last 200 entries
            if (buffer.Count > 200) buffer.RemoveAt(0);

            if (_selectedTask?.TaskName == task.TaskName)
                RedrawLog(task);

            // Update task row status dot
            foreach (var row in _taskRows)
                if (row.Task.TaskName == task.TaskName)
                    row.Refresh();
        }

        private void RedrawLog(TaskBase task)
        {
            rtbLog.Clear();
            if (!_logs.ContainsKey(task.TaskName)) return;

            foreach (var (msg, level, time) in _logs[task.TaskName])
            {
                Color color = level switch
                {
                    TaskBase.LogLevel.Success => C_SUCCESS,
                    TaskBase.LogLevel.Warning => C_WARNING,
                    TaskBase.LogLevel.Error => C_ERROR,
                    _ => C_TEXT_MUTED
                };

                string prefix = $"[{time:HH:mm:ss}]  ";
                rtbLog.SelectionStart = rtbLog.TextLength;
                rtbLog.SelectionLength = 0;
                rtbLog.SelectionColor = C_TEXT_MUTED;
                rtbLog.AppendText(prefix);
                rtbLog.SelectionColor = color;
                rtbLog.AppendText(msg + "\n");
            }

            // Scroll to bottom
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.ScrollToCaret();
        }

        // ── UI Refresh ────────────────────────────────────────────────────────────

        private void RefreshUI(TaskBase task)
        {
            if (InvokeRequired)
            {
                Invoke(() => RefreshUI(task));
                return;
            }

            foreach (var row in _taskRows)
                if (row.Task.TaskName == task.TaskName)
                    row.Refresh();

            if (_selectedTask?.TaskName == task.TaskName)
                RefreshDetail(task);
        }

        // ── Theme Setup ───────────────────────────────────────────────────────────

        private void SetupTheme()
        {
            BackColor = C_BG;
            ForeColor = C_TEXT;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _httpServer?.Stop();
            foreach (var task in _tasks)
                task.Stop();
            base.OnFormClosing(e);
        }
    }

    // ── TaskRow — custom left panel item ─────────────────────────────────────────

    public class TaskRow : Panel
    {
        public TaskBase Task { get; }
        private bool _selected;

        private static readonly Color C_BG_NORMAL = ColorTranslator.FromHtml("#1e2130");
        private static readonly Color C_BG_SELECTED = ColorTranslator.FromHtml("#1e1535");
        private static readonly Color C_PRIMARY = ColorTranslator.FromHtml("#7c4dff");
        private static readonly Color C_TEXT = ColorTranslator.FromHtml("#e8eaf0");
        private static readonly Color C_TEXT_MUTED = ColorTranslator.FromHtml("#8b8fa8");
        private static readonly Color C_SUCCESS = ColorTranslator.FromHtml("#059669");
        private static readonly Color C_ERROR = ColorTranslator.FromHtml("#ef4444");
        private static readonly Color C_SURFACE_LOW = ColorTranslator.FromHtml("#252838");

        public TaskRow(TaskBase task)
        {
            Task = task;
            Height = 64;
            Dock = DockStyle.Top;
            Padding = new Padding(16, 0, 16, 0);
            BackColor = C_BG_NORMAL;
            DoubleBuffered = true;
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            BackColor = selected ? C_BG_SELECTED : C_BG_NORMAL;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Selected left accent bar
            if (_selected)
                g.FillRectangle(new SolidBrush(C_PRIMARY), 0, 12, 3, Height - 24);

            // Status dot
            Color dotColor = Task.IsRunning ? C_SUCCESS : C_ERROR;
            g.FillEllipse(new SolidBrush(dotColor), 20, (Height / 2) - 4, 8, 8);

            // Task name
            using var nameFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            g.DrawString(Task.TaskName, nameFont, new SolidBrush(_selected ? C_PRIMARY : C_TEXT), 36, (Height / 2) - 12);

            // Interval label
            using var subFont = new Font("Segoe UI", 7.5f, FontStyle.Regular);
            g.DrawString($"every {Task.IntervalSeconds}s", subFont, new SolidBrush(C_TEXT_MUTED), 36, (Height / 2) + 4);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (!_selected) BackColor = ColorTranslator.FromHtml("#252838");
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!_selected) BackColor = C_BG_NORMAL;
        }
    }
}