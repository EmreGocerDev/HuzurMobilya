using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HuzurMobilya.Models;
using Newtonsoft.Json;

namespace HuzurMobilya.Forms
{
    public class AgendaPage : UserControl
    {
        private MonthCalendar calendar = null!;
        private ListBox lstNotes = null!;
        private Label lblDateHeader = null!;
        private List<AgendaNote> allNotes = new();
        private DateTime selectedDate = DateTime.Today;

        private static readonly string DataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HuzurMobilya");
        private static readonly string DataFile = Path.Combine(DataDir, "agenda.json");

        public AgendaPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            // ── Toolbar ──
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56, FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8), BackColor = Theme.Surface
            };
            var btnAdd = Theme.CreateButton("+ Not Ekle", Theme.Primary, 140);
            btnAdd.Click += BtnAdd_Click;
            var btnDelete = Theme.CreateButton("🗑 Sil", Theme.Danger, 100);
            btnDelete.Click += BtnDelete_Click;
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnDelete });
            Controls.Add(toolbar);

            // ── Split ── (robust splitter distance to avoid runtime exception on small widths)
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Background
            };
            // sensible minimums will be applied on Load to avoid triggering SplitterDistance during ctor

            // Helper to safely set splitter distance based on current width
            void UpdateSplitter()
            {
                try
                {
                    var desired = 280;
                    var min1 = Math.Max(100, split.Panel1MinSize);
                    var min2 = Math.Max(100, split.Panel2MinSize);
                    var total = split.Width > 0 ? split.Width : this.ClientSize.Width;
                    if (total <= 0) return;
                    var maxDist = Math.Max(0, total - min2);
                    var dist = Math.Clamp(desired, min1, Math.Max(min1, maxDist));
                    // ensure dist is within absolute bounds
                    if (dist < min1) dist = min1;
                    if (dist > total - min2) dist = Math.Max(min1, total - min2);
                    split.SplitterDistance = dist;
                }
                catch { /* swallow any timing issues during layout */ }
            }

            // Apply min sizes and update on load and when user/resizing occurs
            this.Load += (s, e) =>
            {
                try
                {
                    split.Panel1MinSize = 160;
                    split.Panel2MinSize = 160;
                }
                catch { }
                UpdateSplitter();
            };
            this.Resize += (s, e) => UpdateSplitter();

            // Left: Calendar
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(16) };

            calendar = new MonthCalendar
            {
                MaxSelectionCount = 1,
                BackColor = Color.White,
                ForeColor = Theme.TextPrimary,
                TitleBackColor = Theme.Primary,
                TitleForeColor = Color.White,
                TrailingForeColor = Theme.TextSecondary,
                ShowTodayCircle = true,
                Font = new Font("Segoe UI", 10)
            };
            calendar.DateSelected += Calendar_DateSelected;

            // Put calendar inside a small card for nicer appearance
            var calCard = new Panel { Location = new Point(16, 16), Size = new Size(360, 220), BackColor = Theme.Surface };
            calCard.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var sb = new SolidBrush(Theme.Surface);
                using var pen = new Pen(Theme.Border, 1);
                var rect = new Rectangle(0, 0, calCard.Width - 1, calCard.Height - 1);
                g.FillPath(sb, Theme.RoundedRect(rect, 12));
                g.DrawPath(pen, Theme.RoundedRect(rect, 12));
            };
            calendar.Location = new Point(12, 8);
            calCard.Controls.Add(calendar);
            leftPanel.Controls.Add(calCard);

            // Takvimde notlu günleri işaretle
            var legendPanel = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.TopDown,
                Location = new Point(16, 240), BackColor = Color.Transparent
            };
            void AddLegend(Color c, string text)
            {
                var p = new Panel { Width = 180, Height = 22, BackColor = Color.Transparent };
                p.Controls.Add(new Panel { Size = new Size(14, 14), Location = new Point(0, 4), BackColor = c });
                p.Controls.Add(new Label { Text = text, Location = new Point(20, 2), AutoSize = true, Font = Theme.SmallFont, ForeColor = Theme.TextSecondary });
                legendPanel.Controls.Add(p);
            }
            AddLegend(Color.FromArgb(99, 102, 241), "Genel");
            AddLegend(Color.FromArgb(16, 185, 129), "Toplantı");
            AddLegend(Color.FromArgb(245, 158, 11), "Hatırlatma");
            AddLegend(Color.FromArgb(239, 68, 68), "Önemli");
            leftPanel.Controls.Add(legendPanel);

            split.Panel1.Controls.Add(leftPanel);

            // Right: Notes list
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(16) };

            lblDateHeader = new Label
            {
                Text = DateTime.Today.ToString("dd MMMM yyyy, dddd"),
                Font = Theme.SubtitleFont, ForeColor = Theme.Primary,
                AutoSize = true, Location = new Point(0, 0)
            };
            rightPanel.Controls.Add(lblDateHeader);

            var lblNotesList = new Label
            {
                Text = "Bu Güne Ait Notlar:", Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(0, 34)
            };
            rightPanel.Controls.Add(lblNotesList);

            lstNotes = new ListBox
            {
                Location = new Point(0, 58), Font = Theme.BodyFont,
                BorderStyle = BorderStyle.None, BackColor = Color.White,
                DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 68,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Size = new Size(rightPanel.Width - 32, rightPanel.Height - 80)
            };
            lstNotes.DrawItem += LstNotes_DrawItem;
            lstNotes.DoubleClick += LstNotes_DoubleClick;
            rightPanel.Controls.Add(lstNotes);

            rightPanel.Resize += (s, e) =>
            {
                lstNotes.Size = new Size(rightPanel.Width - 32, rightPanel.Height - 80);
            };

            split.Panel2.Controls.Add(rightPanel);
            Controls.Add(split);
            split.BringToFront(); toolbar.BringToFront();

            LoadNotes();
            RefreshDayView();
        }

        private void LoadNotes()
        {
            try
            {
                if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);
                if (File.Exists(DataFile))
                    allNotes = JsonConvert.DeserializeObject<List<AgendaNote>>(File.ReadAllText(DataFile)) ?? new();
                else
                    allNotes = new();
            }
            catch { allNotes = new(); }

            // Takvimde notlu günleri Bold ile göster
            var boldDates = allNotes.Select(n => n.Date.Date).Distinct().ToArray();
            calendar.BoldedDates = boldDates;
            calendar.UpdateBoldedDates();
        }

        private void SaveNotes()
        {
            if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);
            File.WriteAllText(DataFile, JsonConvert.SerializeObject(allNotes, Formatting.Indented));
        }

        private void RefreshDayView()
        {
            lblDateHeader.Text = selectedDate.ToString("dd MMMM yyyy, dddd");
            lstNotes.Items.Clear();
            var dayNotes = allNotes.Where(n => n.Date.Date == selectedDate.Date)
                                   .OrderBy(n => n.CreatedAt).ToList();
            foreach (var note in dayNotes) lstNotes.Items.Add(note);
        }

        private void Calendar_DateSelected(object? sender, DateRangeEventArgs e)
        {
            selectedDate = e.Start.Date;
            RefreshDayView();
        }

        private void LstNotes_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstNotes.Items.Count) return;
            var note = lstNotes.Items[e.Index] as AgendaNote;
            if (note == null) return;

            e.DrawBackground();
            var g = e.Graphics;

            var tagColor = note.Tag switch
            {
                "toplanti"   => Color.FromArgb(16, 185, 129),
                "hatirlatma" => Color.FromArgb(245, 158, 11),
                "onemli"     => Color.FromArgb(239, 68, 68),
                _ => Color.FromArgb(99, 102, 241)
            };
            g.FillRectangle(new SolidBrush(Color.FromArgb(30, tagColor)), e.Bounds);
            g.FillRectangle(new SolidBrush(tagColor), new Rectangle(e.Bounds.X, e.Bounds.Y + 8, 4, e.Bounds.Height - 16));

            var titleRect = new Rectangle(e.Bounds.X + 12, e.Bounds.Y + 8, e.Bounds.Width - 16, 22);
            var contentRect = new Rectangle(e.Bounds.X + 12, e.Bounds.Y + 32, e.Bounds.Width - 16, 32);

            g.DrawString(note.Title, new Font("Segoe UI", 10, FontStyle.Bold), new SolidBrush(Theme.TextPrimary), titleRect);
            if (!string.IsNullOrEmpty(note.Content))
                g.DrawString(note.Content, Theme.SmallFont, new SolidBrush(Theme.TextSecondary), contentRect);

            e.DrawFocusRectangle();
        }

        private void LstNotes_DoubleClick(object? sender, EventArgs e)
        {
            if (lstNotes.SelectedItem is not AgendaNote note) return;
            var f = new AgendaNoteForm(note);
            if (f.ShowDialog() == DialogResult.OK)
            {
                var existing = allNotes.FirstOrDefault(n => n.Id == note.Id);
                if (existing != null) { existing.Title = note.Title; existing.Content = note.Content; existing.Tag = note.Tag; }
                SaveNotes();
                LoadNotes();
                RefreshDayView();
            }
            else if (f.WasDeleted)
            {
                allNotes.RemoveAll(n => n.Id == note.Id);
                SaveNotes();
                LoadNotes();
                RefreshDayView();
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var newNote = new AgendaNote { Date = selectedDate };
            var f = new AgendaNoteForm(newNote);
            if (f.ShowDialog() == DialogResult.OK)
            {
                allNotes.Add(newNote);
                SaveNotes();
                LoadNotes();
                RefreshDayView();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lstNotes.SelectedItem is not AgendaNote note) return;
            if (MessageBox.Show($"'{note.Title}' notunu silmek istiyor musunuz?", "Sil",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            allNotes.RemoveAll(n => n.Id == note.Id);
            SaveNotes();
            LoadNotes();
            RefreshDayView();
        }
    }

    public class AgendaNoteForm : Form
    {
        private AgendaNote _note;
        private TextBox txtTitle = null!, txtContent = null!;
        private ComboBox cmbTag = null!;
        public bool WasDeleted { get; private set; }

        public AgendaNoteForm(AgendaNote note)
        {
            _note = note;
            Text = string.IsNullOrEmpty(note.Title) ? "Yeni Not" : "Notu Düzenle";
            Size = new Size(480, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            var header = Theme.CreateModernHeader("📅", string.IsNullOrEmpty(note.Title) ? "Yeni Not" : "Notu Düzenle");
            Controls.Add(header);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            Controls.Add(panel);
            panel.BringToFront(); header.BringToFront();

            int y = header.Height + 12; // leave space under the header so inputs aren't hidden
            panel.Controls.Add(new Label { Text = "Başlık *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(0, y) });
            y += 20;
            txtTitle = Theme.CreateTextBox(note.Title, 420);
            txtTitle.Location = new Point(0, y); panel.Controls.Add(txtTitle);
            y += 36;

            panel.Controls.Add(new Label { Text = "İçerik", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(0, y) });
            y += 20;
            txtContent = new TextBox
            {
                Multiline = true, Height = 100, Width = 420, ScrollBars = ScrollBars.Vertical,
                Font = Theme.BodyFont, Text = note.Content, Location = new Point(0, y)
            };
            panel.Controls.Add(txtContent);
            y += 110;

            panel.Controls.Add(new Label { Text = "Etiket", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(0, y) });
            y += 20;
            cmbTag = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont, Width = 200, Location = new Point(0, y) };
            cmbTag.Items.AddRange(new object[] { "genel", "toplanti", "hatirlatma", "onemli" });
            cmbTag.Text = note.Tag;
            panel.Controls.Add(cmbTag);
            y += 44;

            var btnSave = Theme.CreateButton("💾 Kaydet", Theme.Success, 150, 40);
            btnSave.Location = new Point(0, y);
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text)) { MessageBox.Show("Başlık giriniz."); return; }
                note.Title = txtTitle.Text.Trim();
                note.Content = txtContent.Text.Trim();
                note.Tag = cmbTag.Text;
                DialogResult = DialogResult.OK; Close();
            };
            panel.Controls.Add(btnSave);

            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 110, 40);
            btnCancel.Location = new Point(160, y);
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            panel.Controls.Add(btnCancel);
        }
    }
}
