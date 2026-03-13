using ConsoleApp2;

namespace ExamApp.Controls;

public class TeacherDashControl : UserControl
{
    public TeacherDashControl(int initialTab = 0)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.BgDark;
        Padding = new Padding(40, 28, 40, 28);

        // ── Tab buttons ──
        var tabBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent,
            WrapContents = false,
        };

        var tabs = new[] { "👥  Students", "📋  All Results", "📈  Performance" };
        var tabButtons = new Button[tabs.Length];
        var tabPanels = new Panel[tabs.Length];

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
        };

        for (int i = 0; i < tabs.Length; i++)
        {
            int idx = i;
            var btn = new Button
            {
                Text = tabs[i],
                Font = Theme.FontNav,
                Width = 180,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Theme.TextSecondary,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0),
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 99, 102, 241);
            btn.Click += (_, _) => SwitchTab(idx, tabButtons, tabPanels);
            tabButtons[i] = btn;
            tabBar.Controls.Add(btn);
        }

        Controls.Add(contentPanel);
        Controls.Add(tabBar);

        // ── Tab panels ──
        tabPanels[0] = BuildStudentsTab();
        tabPanels[1] = BuildResultsTab();
        tabPanels[2] = BuildPerformanceTab();

        foreach (var p in tabPanels)
        {
            p.Dock = DockStyle.Fill;
            p.Visible = false;
            contentPanel.Controls.Add(p);
        }

        SwitchTab(initialTab, tabButtons, tabPanels);
    }

    private static void SwitchTab(int index, Button[] buttons, Panel[] panels)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            bool active = i == index;
            buttons[i].ForeColor = active ? Theme.Accent : Theme.TextSecondary;
            buttons[i].BackColor = active ? Color.FromArgb(40, 99, 102, 241) : Color.Transparent;
            panels[i].Visible = active;
        }
    }

    // ── Students tab ──
    private static Panel BuildStudentsTab()
    {
        var panel = new Panel { BackColor = Color.Transparent, Padding = new Padding(0, 16, 0, 0) };

        var students = AuthService.GetAllUsers(UserRole.Student);

        if (students.Length == 0)
        {
            var lbl = Theme.MakeLabel("No students registered yet.", Theme.FontBody, Theme.TextMuted);
            lbl.Dock = DockStyle.Top;
            panel.Controls.Add(lbl);
            return panel;
        }

        var grid = Theme.MakeGrid();
        grid.Columns.Add("No", "#");
        grid.Columns.Add("Name", "Name");
        grid.Columns.Add("Email", "Email");
        grid.Columns["No"]!.Width = 50;
        grid.Columns["No"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

        for (int i = 0; i < students.Length; i++)
            grid.Rows.Add(i + 1, students[i].Name, students[i].Email);

        var lblCount = Theme.MakeLabel($"Total: {students.Length} student(s)", Theme.FontSmall, Theme.TextMuted);
        lblCount.Dock = DockStyle.Bottom;
        lblCount.Height = 32;
        lblCount.TextAlign = ContentAlignment.MiddleLeft;

        panel.Controls.Add(grid);
        panel.Controls.Add(lblCount);
        return panel;
    }

    // ── Results tab ──
    private static Panel BuildResultsTab()
    {
        var panel = new Panel { BackColor = Color.Transparent, Padding = new Padding(0, 16, 0, 0) };

        var grades = ResultLogger.ReadAllGrades();

        if (grades.Length == 0)
        {
            var lbl = Theme.MakeLabel("No exam results found.", Theme.FontBody, Theme.TextMuted);
            lbl.Dock = DockStyle.Top;
            panel.Controls.Add(lbl);
            return panel;
        }

        var grid = Theme.MakeGrid();
        grid.Columns.Add("No", "#");
        grid.Columns.Add("Student", "Student");
        grid.Columns.Add("Subject", "Subject");
        grid.Columns.Add("Type", "Type");
        grid.Columns.Add("Score", "Score");
        grid.Columns.Add("Pct", "%");
        grid.Columns.Add("Date", "Date");
        grid.Columns["No"]!.Width = 50;
        grid.Columns["No"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

        for (int i = 0; i < grades.Length; i++)
        {
            var g = grades[i];
            double pct = g.Total > 0 ? (double)g.Score / g.Total * 100 : 0;
            grid.Rows.Add(i + 1, g.StudentName, g.SubjectName, g.ExamType,
                $"{g.Score}/{g.Total}", $"{pct:F0}%", g.Date);
        }

        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 5) return;
            var pctStr = e.Value?.ToString() ?? "0";
            double.TryParse(pctStr.TrimEnd('%'), out double val);
            if (val >= 80) e.CellStyle!.ForeColor = Theme.Success;
            else if (val >= 50) e.CellStyle!.ForeColor = Theme.Warning;
            else e.CellStyle!.ForeColor = Theme.Danger;
        };

        panel.Controls.Add(grid);
        return panel;
    }

    // ── Performance tab ──
    private static Panel BuildPerformanceTab()
    {
        var panel = new Panel { BackColor = Color.Transparent, Padding = new Padding(0, 16, 0, 0), AutoScroll = true };

        var grades = ResultLogger.ReadAllGrades();

        if (grades.Length == 0)
        {
            var lbl = Theme.MakeLabel("No performance data available.", Theme.FontBody, Theme.TextMuted);
            lbl.Dock = DockStyle.Top;
            panel.Controls.Add(lbl);
            return panel;
        }

        // aggregate by student
        var seen = new Dictionary<string, (string Name, int TotalScore, int TotalMax, int Count)>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var g in grades)
        {
            if (seen.TryGetValue(g.Email, out var agg))
                seen[g.Email] = (agg.Name, agg.TotalScore + g.Score, agg.TotalMax + g.Total, agg.Count + 1);
            else
                seen[g.Email] = (g.StudentName, g.Score, g.Total, 1);
        }

        var lblTitle = Theme.MakeLabel("Student Performance Summary", Theme.FontSubtitle);
        lblTitle.Dock = DockStyle.Top;
        lblTitle.Height = 44;
        panel.Controls.Add(lblTitle);

        var grid = Theme.MakeGrid();
        grid.Columns.Add("Student", "Student");
        grid.Columns.Add("Exams", "Exams Taken");
        grid.Columns.Add("TotalScore", "Total Score");
        grid.Columns.Add("Avg", "Average %");
        grid.Columns["Exams"]!.Width = 100;
        grid.Columns["Exams"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

        foreach (var kvp in seen)
        {
            var a = kvp.Value;
            double avg = a.TotalMax > 0 ? (double)a.TotalScore / a.TotalMax * 100 : 0;
            grid.Rows.Add(a.Name, a.Count, $"{a.TotalScore}/{a.TotalMax}", $"{avg:F1}%");
        }

        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 3) return;
            var avgStr = e.Value?.ToString() ?? "0";
            double.TryParse(avgStr.TrimEnd('%'), out double val);
            if (val >= 80) e.CellStyle!.ForeColor = Theme.Success;
            else if (val >= 50) e.CellStyle!.ForeColor = Theme.Warning;
            else e.CellStyle!.ForeColor = Theme.Danger;
        };

        // Bar chart visual
        var barPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 180,
            BackColor = Theme.BgCard,
            Padding = new Padding(20),
        };

        barPanel.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int barWidth = 60;
            int gap = 20;
            int x = 30;
            int maxHeight = barPanel.Height - 60;
            int baseY = barPanel.Height - 30;

            foreach (var kvp in seen)
            {
                var a = kvp.Value;
                double avg = a.TotalMax > 0 ? (double)a.TotalScore / a.TotalMax * 100 : 0;
                int barH = (int)(avg / 100 * maxHeight);
                Color color = avg >= 80 ? Theme.Success : avg >= 50 ? Theme.Warning : Theme.Danger;

                using var brush = new SolidBrush(Color.FromArgb(200, color));
                g.FillRectangle(brush, x, baseY - barH, barWidth, barH);

                using var textBrush = new SolidBrush(Theme.TextPrimary);
                var nameSize = g.MeasureString(a.Name, Theme.FontSmall);
                g.DrawString(a.Name, Theme.FontSmall, textBrush,
                    x + (barWidth - nameSize.Width) / 2, baseY + 4);
                g.DrawString($"{avg:F0}%", Theme.FontSmall, textBrush,
                    x + (barWidth - g.MeasureString($"{avg:F0}%", Theme.FontSmall).Width) / 2,
                    baseY - barH - 18);

                x += barWidth + gap;
            }
        };

        panel.Controls.Add(grid);
        panel.Controls.Add(barPanel);
        return panel;
    }
}
