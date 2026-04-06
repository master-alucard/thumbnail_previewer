using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ThumbnailPreviewer.Infrastructure;

namespace ThumbnailPreviewer.Settings
{
    public class SettingsForm : Form
    {
        private DataGridView _grid;
        private Button _btnSave;
        private Button _btnReset;
        private Label _lblStatus;

        // Column indices
        private const int ColExtension = 0;
        private const int ColDescription = 1;
        private const int ColPreview = 2;
        private const int ColBadge = 3;

        public SettingsForm()
        {
            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            Text = "ThumbnailPreviewer Settings";
            Size = new Size(580, 560);
            MinimumSize = new Size(480, 400);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9f);

            // Set window icon from embedded EXE resource
            try { Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location); }
            catch { }

            // Header label
            var lblHeader = new Label
            {
                Text = "Configure which file types show thumbnail previews and extension badges in Windows Explorer.",
                Dock = DockStyle.Top,
                Padding = new Padding(12, 10, 12, 6),
                AutoSize = true,
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            // DataGridView
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
                Name = "Extension",
                HeaderText = "Extension",
                ReadOnly = true,
                FillWeight = 18,
                DefaultCellStyle = { Font = new Font("Consolas", 9.5f, FontStyle.Bold) }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                HeaderText = "Description",
                ReadOnly = true,
                FillWeight = 42
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Preview",
                HeaderText = "Preview",
                FillWeight = 20
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Badge",
                HeaderText = "Badge",
                FillWeight = 20
            });

            // Bottom panel with buttons
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10, 8, 10, 8)
            };

            _btnSave = new Button
            {
                Text = "Save",
                Size = new Size(90, 32),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                FlatStyle = FlatStyle.System
            };
            _btnSave.Click += BtnSave_Click;

            // Commit checkbox changes immediately (not on row leave)
            _grid.CurrentCellDirtyStateChanged += (s, ev) =>
            {
                if (_grid.IsCurrentCellDirty)
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // When Preview is unchecked, disable and uncheck Badge
            _grid.CellValueChanged += Grid_CellValueChanged;

            _btnReset = new Button
            {
                Text = "Reset Defaults",
                Size = new Size(110, 32),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                FlatStyle = FlatStyle.System
            };
            _btnReset.Click += BtnReset_Click;

            _lblStatus = new Label
            {
                Text = "",
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                ForeColor = Color.FromArgb(0, 120, 0),
                Location = new Point(12, 16)
            };

            // Layout
            _btnSave.Location = new Point(panelBottom.Width - 105, 9);
            _btnReset.Location = new Point(panelBottom.Width - 225, 9);

            panelBottom.Controls.Add(_btnSave);
            panelBottom.Controls.Add(_btnReset);
            panelBottom.Controls.Add(_lblStatus);

            // Reposition buttons on resize
            panelBottom.Resize += (s, e) =>
            {
                _btnSave.Location = new Point(panelBottom.Width - 105, 9);
                _btnReset.Location = new Point(panelBottom.Width - 225, 9);
            };

            Controls.Add(_grid);
            Controls.Add(panelBottom);
            Controls.Add(lblHeader);
        }

        private void LoadSettings()
        {
            _grid.Rows.Clear();

            foreach (var ext in SettingsManager.AllExtensions)
            {
                bool preview = SettingsManager.IsPreviewEnabled(ext.Extension);
                bool badge = SettingsManager.IsBadgeEnabled(ext.Extension);

                int row = _grid.Rows.Add(
                    $".{ext.Extension}",
                    ext.Description,
                    preview,
                    badge
                );

                // If preview is off, disable badge cell
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
                // Kill COM surrogates holding our DLL
                foreach (var p in Process.GetProcessesByName("dllhost"))
                {
                    try { p.Kill(); } catch { }
                }

                // Stop Explorer so it releases thumbnail cache file locks
                foreach (var p in Process.GetProcessesByName("explorer"))
                {
                    try { p.Kill(); } catch { }
                }

                // Brief wait for file handles to release
                System.Threading.Thread.Sleep(1500);

                // Delete thumbnail cache files (now unlocked)
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

                // Restart Explorer
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
