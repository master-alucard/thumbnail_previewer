using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using ThumbnailPreviewer.Infrastructure;

namespace ThumbnailPreviewer.Settings
{
    public class SettingsForm : Form
    {
        private TabControl _tabs;
        private DataGridView _grid;
        private Button _btnSave;
        private Button _btnReset;
        private Label _lblStatus;

        // Column indices
        private const int ColExtension = 0;
        private const int ColDescription = 1;
        private const int ColPreview = 2;
        private const int ColBadge = 3;

        // Branding colors (Material Design blue - matches deduper)
        private static readonly Color AccentColor = Color.FromArgb(21, 101, 192);  // #1565C0
        private static readonly Color AccentLight = Color.FromArgb(100, 160, 220);

        public SettingsForm()
        {
            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            Text = "ThumbnailPreviewer Settings";
            Size = new Size(580, 580);
            MinimumSize = new Size(480, 420);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9f);

            try { Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location); }
            catch { }

            // Tab control
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            _tabs.TabPages.Add(CreateSettingsTab());
            _tabs.TabPages.Add(CreateAboutTab());

            Controls.Add(_tabs);
        }

        // ==============================================================
        // Settings Tab
        // ==============================================================
        private TabPage CreateSettingsTab()
        {
            var tab = new TabPage("Settings");

            var lblHeader = new Label
            {
                Text = "Configure which file types show thumbnail previews and extension badges in Windows Explorer.",
                Dock = DockStyle.Top,
                Padding = new Padding(12, 10, 12, 6),
                AutoSize = true,
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 32,
                RowTemplate = { Height = 26 },
                DefaultCellStyle = { SelectionBackColor = Color.FromArgb(230, 240, 255), SelectionForeColor = Color.Black }
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Extension", HeaderText = "Extension", ReadOnly = true, FillWeight = 18,
                DefaultCellStyle = { Font = new Font("Consolas", 9.5f, FontStyle.Bold) }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description", HeaderText = "Description", ReadOnly = true, FillWeight = 42
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Preview", HeaderText = "Preview", FillWeight = 20
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Badge", HeaderText = "Badge", FillWeight = 20
            });

            _grid.CurrentCellDirtyStateChanged += (s, ev) =>
            {
                if (_grid.IsCurrentCellDirty)
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _grid.CellValueChanged += Grid_CellValueChanged;

            // Bottom panel
            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10, 8, 10, 8) };

            _btnSave = new Button
            {
                Text = "Save", Size = new Size(90, 32),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom, FlatStyle = FlatStyle.System
            };
            _btnSave.Click += BtnSave_Click;

            _btnReset = new Button
            {
                Text = "Reset Defaults", Size = new Size(110, 32),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom, FlatStyle = FlatStyle.System
            };
            _btnReset.Click += BtnReset_Click;

            _lblStatus = new Label
            {
                Text = "", AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                ForeColor = Color.FromArgb(0, 120, 0), Location = new Point(12, 16)
            };

            _btnSave.Location = new Point(panelBottom.Width - 105, 9);
            _btnReset.Location = new Point(panelBottom.Width - 225, 9);

            panelBottom.Controls.Add(_btnSave);
            panelBottom.Controls.Add(_btnReset);
            panelBottom.Controls.Add(_lblStatus);

            panelBottom.Resize += (s, e) =>
            {
                _btnSave.Location = new Point(panelBottom.Width - 105, 9);
                _btnReset.Location = new Point(panelBottom.Width - 225, 9);
            };

            tab.Controls.Add(_grid);
            tab.Controls.Add(panelBottom);
            tab.Controls.Add(lblHeader);

            return tab;
        }

        // ==============================================================
        // About Tab
        // ==============================================================
        private TabPage CreateAboutTab()
        {
            var tab = new TabPage("About");
            tab.BackColor = Color.White;

            // Hero panel (blue header)
            var hero = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BackColor = AccentColor
            };
            hero.Paint += (s, e) =>
            {
                // Subtle gradient overlay
                using (var brush = new LinearGradientBrush(hero.ClientRectangle,
                    Color.FromArgb(30, 0, 0, 0), Color.Transparent, 90f))
                {
                    e.Graphics.FillRectangle(brush, hero.ClientRectangle);
                }
            };

            var lblAppName = new Label
            {
                Text = "ThumbnailPreviewer",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(30, 25)
            };

            var lblVersion = new Label
            {
                Text = "Version 1.0.0",
                Font = new Font("Segoe UI", 10f),
                ForeColor = AccentLight,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(32, 60)
            };

            var lblCopyright = new Label
            {
                Text = "\u00a9 2026 Katador.net  \u00b7  All rights reserved.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(180, 200, 230),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(32, 85)
            };

            var lblEmail = new Label
            {
                Text = "office@katador.net",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(180, 200, 230),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(32, 105),
                Cursor = Cursors.Hand
            };
            lblEmail.Click += (s, e) => OpenUrl("mailto:office@katador.net");

            // Action buttons
            var btnGitHub = CreateHeroButton("GitHub", 30, 130);
            btnGitHub.Click += (s, e) => OpenUrl("https://github.com/master-alucard/thumbnail_previewer");

            var btnWebsite = CreateHeroButton("katador.net", 120, 130);
            btnWebsite.Click += (s, e) => OpenUrl("https://katador.net");

            var btnEmail = CreateHeroButton("Email", 230, 130);
            btnEmail.Click += (s, e) => OpenUrl("mailto:office@katador.net");

            hero.Controls.Add(lblAppName);
            hero.Controls.Add(lblVersion);
            hero.Controls.Add(lblCopyright);
            hero.Controls.Add(lblEmail);
            hero.Controls.Add(btnGitHub);
            hero.Controls.Add(btnWebsite);
            hero.Controls.Add(btnEmail);

            // Description panel
            var descPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 20, 30, 20),
                BackColor = Color.White
            };

            var lblDesc = new Label
            {
                Text = "Windows Explorer shell extension that generates thumbnail previews " +
                       "for file formats not natively supported by Windows.\n\n" +
                       "Supported formats:\n" +
                       "PDF, DOCX, PSD, AI, EPS, SVG, CSV/TSV,\n" +
                       "OpenDocument (ODT, ODS, ODP, ODG),\n" +
                       "Camera RAW (DNG, CR2, CR3, NEF, ARW, ORF, RW2, RAF, SRW, PEF)\n\n" +
                       "Built with SharpShell, Magick.NET, PDFtoImage, and Ghostscript.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(60, 60, 60),
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            descPanel.Controls.Add(lblDesc);

            tab.Controls.Add(descPanel);
            tab.Controls.Add(hero);

            return tab;
        }

        private LinkLabel CreateHeroButton(string text, int x, int y)
        {
            var btn = new LinkLabel
            {
                Text = text,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                LinkColor = Color.White,
                ActiveLinkColor = AccentLight,
                BackColor = Color.FromArgb(50, 255, 255, 255),
                AutoSize = false,
                Size = new Size(95, 24),
                Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                LinkBehavior = LinkBehavior.NeverUnderline
            };
            return btn;
        }

        private static void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { }
        }

        // ==============================================================
        // Settings logic
        // ==============================================================
        private void LoadSettings()
        {
            _grid.Rows.Clear();

            foreach (var ext in SettingsManager.AllExtensions)
            {
                bool preview = SettingsManager.IsPreviewEnabled(ext.Extension);
                bool badge = SettingsManager.IsBadgeEnabled(ext.Extension);

                int row = _grid.Rows.Add($".{ext.Extension}", ext.Description, preview, badge);

                if (!preview)
                {
                    _grid.Rows[row].Cells[ColBadge].ReadOnly = true;
                    _grid.Rows[row].Cells[ColBadge].Style.BackColor = Color.FromArgb(240, 240, 240);
                    _grid.Rows[row].Cells[ColBadge].Style.ForeColor = Color.LightGray;
                }
            }

            _lblStatus.Text = "";
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != ColPreview) return;

            var row = _grid.Rows[e.RowIndex];
            bool previewOn = (bool)(row.Cells[ColPreview].Value ?? true);

            if (!previewOn)
            {
                row.Cells[ColBadge].ReadOnly = true;
                row.Cells[ColBadge].Style.BackColor = Color.FromArgb(240, 240, 240);
                row.Cells[ColBadge].Style.ForeColor = Color.LightGray;
            }
            else
            {
                row.Cells[ColBadge].ReadOnly = false;
                row.Cells[ColBadge].Style.BackColor = Color.White;
                row.Cells[ColBadge].Style.ForeColor = Color.Black;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < _grid.Rows.Count; i++)
            {
                var row = _grid.Rows[i];
                var ext = SettingsManager.AllExtensions[i].Extension;
                var preview = (bool)(row.Cells[ColPreview].Value ?? true);
                var badge = (bool)(row.Cells[ColBadge].Value ?? true);

                SettingsManager.SetPreviewEnabled(ext, preview);
                SettingsManager.SetBadgeEnabled(ext, badge);
            }

            var result = MessageBox.Show(
                "Settings saved.\n\nRestart Explorer now to apply changes?\n(Explorer windows will close briefly)",
                "Restart Explorer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ClearCacheAndRestartExplorer();
                _lblStatus.ForeColor = Color.FromArgb(0, 120, 0);
                _lblStatus.Text = "Settings applied. Explorer restarted.";
            }
            else
            {
                _lblStatus.ForeColor = Color.FromArgb(0, 120, 0);
                _lblStatus.Text = "Settings saved. Restart Explorer to apply.";
            }
        }

        private static void ClearCacheAndRestartExplorer()
        {
            try
            {
                foreach (var p in Process.GetProcessesByName("dllhost"))
                {
                    try { p.Kill(); } catch { }
                }
                foreach (var p in Process.GetProcessesByName("explorer"))
                {
                    try { p.Kill(); } catch { }
                }

                System.Threading.Thread.Sleep(1500);

                var explorerCache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Microsoft\Windows\Explorer");

                if (Directory.Exists(explorerCache))
                {
                    foreach (var file in Directory.GetFiles(explorerCache, "thumbcache_*"))
                    {
                        try { File.Delete(file); } catch { }
                    }
                    foreach (var file in Directory.GetFiles(explorerCache, "iconcache_*"))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }

                Process.Start("explorer.exe");
            }
            catch { }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Reset all settings to defaults? (All previews and badges enabled)",
                "Reset Defaults",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SettingsManager.ResetToDefaults();
                LoadSettings();
                _lblStatus.ForeColor = Color.FromArgb(0, 120, 0);
                _lblStatus.Text = "Settings reset to defaults.";
            }
        }
    }
}
