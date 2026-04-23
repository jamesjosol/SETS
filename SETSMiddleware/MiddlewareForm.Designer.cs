using System;
using System.Drawing;
using System.Windows.Forms;

namespace SETSMiddleware
{
    partial class MiddlewareForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Controls ──────────────────────────────────────────────────────────────

        // Top bar
        private Panel pnlTopBar;
        private Label lblAppTitle;
        private Label lblBranchBadge;
        private Label lblOverallStatus;

        // Layout
        private Panel pnlMain;
        private Panel pnlLeft;
        private Panel pnlDivider;
        private Panel pnlRight;

        // Left panel
        private Label lblTasksHeader;
        private Panel pnlTaskList;

        // Right panel — detail header
        private Panel pnlDetailHeader;
        private Label lblTaskTitle;
        private Label lblTaskDescription;
        private Panel pnlMeta;
        private Label lblInterval;
        private Label lblLastRun;
        private Label lblStatus;

        // Right panel — action buttons
        private Panel pnlActions;
        private Button btnToggle;
        private Button btnRunNow;

        // Right panel — log
        private Panel pnlLogHeader;
        private Label lblLogTitle;
        private RichTextBox rtbLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // ── Form ─────────────────────────────────────────────────────────────
            Text = "SETS Middleware";
            Size = new Size(980, 640);
            MinimumSize = new Size(820, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = C_BG;
            ForeColor = C_TEXT;
            Font = new Font("Segoe UI", 9f);

            // ── Top Bar ───────────────────────────────────────────────────────────
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = C_SURFACE,
                Padding = new Padding(20, 0, 20, 0)
            };
            pnlTopBar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(C_BORDER, 1),
                    0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);
            };

            lblAppTitle = new Label
            {
                Text = "SETS Middleware",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = C_TEXT,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            lblBranchBadge = new Label
            {
                Text = _branch,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = C_PRIMARY,
                BackColor = C_PRIMARY_SOFT,
                AutoSize = true,
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(165, 16)
            };

            lblOverallStatus = new Label
            {
                Text = "● Active",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = C_SUCCESS,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblOverallStatus.Location = new Point(pnlTopBar.Width - 90, 17);

            pnlTopBar.Controls.AddRange(new Control[] { lblAppTitle, lblBranchBadge, lblOverallStatus });

            // ── Main Panel ────────────────────────────────────────────────────────
            pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                Padding = new Padding(0)
            };

            // ── Left Panel ────────────────────────────────────────────────────────
            pnlLeft = new Panel
            {
                Width = 220,
                Dock = DockStyle.Left,
                BackColor = C_SURFACE,
                Padding = new Padding(0)
            };
            pnlLeft.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(C_BORDER, 1),
                    pnlLeft.Width - 1, 0, pnlLeft.Width - 1, pnlLeft.Height);
            };

            lblTasksHeader = new Label
            {
                Text = "TASKS",
                Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                ForeColor = C_TEXT_MUTED,
                AutoSize = false,
                Height = 36,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                BackColor = C_SURFACE
            };
            lblTasksHeader.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(C_BORDER, 1),
                    0, lblTasksHeader.Height - 1, lblTasksHeader.Width, lblTasksHeader.Height - 1);
                TextRenderer.DrawText(e.Graphics, "TASKS",
                    new Font("Segoe UI", 7f, FontStyle.Bold),
                    new Point(20, 11), C_TEXT_MUTED);
            };

            pnlTaskList = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_SURFACE,
                AutoScroll = true
            };

            pnlLeft.Controls.Add(pnlTaskList);
            pnlLeft.Controls.Add(lblTasksHeader);

            // ── Divider ───────────────────────────────────────────────────────────
            pnlDivider = new Panel
            {
                Width = 1,
                Dock = DockStyle.Left,
                BackColor = C_BORDER
            };

            // ── Right Panel ───────────────────────────────────────────────────────
            pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                Padding = new Padding(28, 24, 28, 20)
            };

            // Detail header
            pnlDetailHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = C_BG
            };

            lblTaskTitle = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = C_TEXT,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            lblTaskDescription = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = C_TEXT_MUTED,
                AutoSize = false,
                Height = 34,
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.TopLeft
            };

            pnlDetailHeader.Controls.Add(lblTaskTitle);
            pnlDetailHeader.Controls.Add(lblTaskDescription);

            // Meta row (interval, last run, status)
            pnlMeta = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = C_SURFACE_LOW,
                Margin = new Padding(0, 8, 0, 0)
            };
            pnlMeta.Paint += (s, e) =>
            {
                var g = e.Graphics;
                int x = 16;

                void DrawChip(string text, Color fg, Color bg)
                {
                    var sz = TextRenderer.MeasureText(text, new Font("Segoe UI", 8f, FontStyle.Bold));
                    g.FillRectangle(new SolidBrush(bg), x, 10, sz.Width + 16, 22);
                    TextRenderer.DrawText(g, text, new Font("Segoe UI", 8f, FontStyle.Bold),
                        new Rectangle(x + 8, 10, sz.Width, 22), fg, TextFormatFlags.VerticalCenter);
                    x += sz.Width + 28;
                }

                string intervalText = lblInterval.Text;
                string lastRunText = lblLastRun.Text;
                string statusText = lblStatus.Text;

                DrawChip(intervalText, C_TEXT_MUTED, C_SURFACE_HIGH);
                DrawChip(lastRunText, C_TEXT_MUTED, C_SURFACE_HIGH);
                DrawChip(statusText, C_TEXT, C_SURFACE_HIGH);
            };

            lblInterval = new Label { Visible = false, Text = "Interval: —" };
            lblLastRun = new Label { Visible = false, Text = "Last run: Never" };
            lblStatus = new Label { Visible = false, Text = "—" };
            pnlMeta.Controls.AddRange(new Control[] { lblInterval, lblLastRun, lblStatus });

            // Action buttons
            pnlActions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = C_BG,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnToggle = new Button
            {
                Text = "▶  Start Task",
                Width = 130,
                Height = 34,
                Location = new Point(0, 8),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = C_PRIMARY,
                BackColor = C_PRIMARY_SOFT,
                Cursor = Cursors.Hand
            };
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.Click += btnToggle_Click;

            btnRunNow = new Button
            {
                Text = "⚡  Run Now",
                Width = 110,
                Height = 34,
                Location = new Point(140, 8),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = C_TEXT_MUTED,
                BackColor = C_SURFACE_LOW,
                Cursor = Cursors.Hand
            };
            btnRunNow.FlatAppearance.BorderSize = 0;
            btnRunNow.Click += btnRunNow_Click;

            pnlActions.Controls.Add(btnToggle);
            pnlActions.Controls.Add(btnRunNow);

            // Log section
            pnlLogHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = C_BG
            };
            pnlLogHeader.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(C_BORDER, 1), 0, 31, pnlLogHeader.Width, 31);
                TextRenderer.DrawText(e.Graphics, "LIVE LOG",
                    new Font("Segoe UI", 7f, FontStyle.Bold),
                    new Point(0, 8), C_TEXT_MUTED);
            };

            rtbLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = C_SURFACE,
                ForeColor = C_TEXT_MUTED,
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Padding = new Padding(12)
            };

            // Assemble right panel (add in reverse dock order)
            pnlRight.Controls.Add(rtbLog);
            pnlRight.Controls.Add(pnlLogHeader);
            pnlRight.Controls.Add(pnlActions);
            pnlRight.Controls.Add(pnlMeta);
            pnlRight.Controls.Add(pnlDetailHeader);

            // Assemble main
            pnlMain.Controls.Add(pnlRight);
            pnlMain.Controls.Add(pnlDivider);
            pnlMain.Controls.Add(pnlLeft);

            // Assemble form
            Controls.Add(pnlMain);
            Controls.Add(pnlTopBar);

            ResumeLayout(false);
        }
    }
}