using ConsoleApp2;

namespace ExamApp.Controls;

public class StudentDashControl : UserControl
{
    private static readonly Color[] CardAccents =
    [
        Color.FromArgb(99, 102, 241),   // indigo
        Color.FromArgb(16, 185, 129),   // emerald
        Color.FromArgb(245, 158, 11),   // amber
        Color.FromArgb(236, 72, 153),   // pink
    ];

    private static readonly string[] SubjectIcons = ["📐", "💻", "⚛️", "📚"];

    public StudentDashControl(Subject[] subjects, Action<Subject, bool> startExam)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.BgDark;
        AutoScroll = true;
        Padding = new Padding(40, 32, 40, 32);

        var header = Theme.MakeLabel("Choose a Subject", Theme.FontTitle);
        header.Dock = DockStyle.Top;
        header.Height = 56;

        var sub = Theme.MakeLabel("Select a subject and exam type to begin", Theme.FontBody, Theme.TextSecondary);
        sub.Dock = DockStyle.Top;
        sub.Height = 32;

        var cardPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 20, 0, 0),
            BackColor = Color.Transparent,
        };

        for (int i = 0; i < subjects.Length; i++)
        {
            var subj = subjects[i];
            var accent = CardAccents[i % CardAccents.Length];
            var icon = SubjectIcons[i % SubjectIcons.Length];
            cardPanel.Controls.Add(BuildSubjectCard(subj, accent, icon, startExam));
        }

        Controls.Add(cardPanel);
        Controls.Add(sub);
        Controls.Add(header);
    }

    private static Panel BuildSubjectCard(Subject subj, Color accent, string icon, Action<Subject, bool> startExam)
    {
        var card = new RoundedPanel
        {
            Width = 300,
            Height = 260,
            Margin = new Padding(0, 0, 24, 24),
            BackColor = Theme.BgCard,
            Padding = new Padding(24),
        };

        int y = 20;

        var lblIcon = new Label
        {
            Text = icon,
            Font = Theme.FontBig,
            AutoSize = true,
            Location = new Point(24, y),
            BackColor = Color.Transparent,
        };
        card.Controls.Add(lblIcon);
        y += 50;

        var lblName = new Label
        {
            Text = subj.Name,
            Font = Theme.FontSubtitle,
            ForeColor = accent,
            AutoSize = true,
            Location = new Point(24, y),
            BackColor = Color.Transparent,
        };
        card.Controls.Add(lblName);
        y += 32;

        string info = $"Practice: {subj.PracticalExam?.NumberOfQuestions ?? 0}Q · {subj.PracticalExam?.Time ?? 0} min\n" +
                      $"Final: {subj.FinalExam?.NumberOfQuestions ?? 0}Q · {subj.FinalExam?.Time ?? 0} min";
        var lblInfo = new Label
        {
            Text = info,
            Font = Theme.FontSmall,
            ForeColor = Theme.TextSecondary,
            AutoSize = true,
            Location = new Point(24, y),
            BackColor = Color.Transparent,
        };
        card.Controls.Add(lblInfo);
        y += 52;

        var btnPractice = new Button
        {
            Text = "▶  Practice",
            Font = Theme.FontButton,
            Width = 120,
            Height = 36,
            Location = new Point(24, y),
            FlatStyle = FlatStyle.Flat,
            BackColor = accent,
            ForeColor = Theme.TextPrimary,
            Cursor = Cursors.Hand,
        };
        btnPractice.FlatAppearance.BorderSize = 0;
        btnPractice.Click += (_, _) => startExam(subj, true);
        card.Controls.Add(btnPractice);

        var btnFinal = new Button
        {
            Text = "🏁  Final",
            Font = Theme.FontButton,
            Width = 120,
            Height = 36,
            Location = new Point(154, y),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, accent.R, accent.G, accent.B),
            ForeColor = accent,
            Cursor = Cursors.Hand,
        };
        btnFinal.FlatAppearance.BorderSize = 1;
        btnFinal.FlatAppearance.BorderColor = accent;
        btnFinal.Click += (_, _) => startExam(subj, false);
        card.Controls.Add(btnFinal);

        return card;
    }
}
