using ConsoleApp2;

namespace ExamApp.Controls;

public class ExamControl : UserControl
{
    private readonly Exam _exam;
    private readonly Subject _subject;
    private readonly Student _student;
    private readonly Repository<Exam> _history;
    private readonly Action<Exam, Subject> _onFinished;

    private int _currentIndex;
    private readonly Dictionary<int, Answer> _answers = new();
    private int _secondsLeft;

    // UI
    private readonly Label _lblTitle;
    private readonly Label _lblTimer;
    private readonly ProgressBar _progressBar;
    private readonly Label _lblProgress;
    private readonly Panel _questionPanel;
    private readonly Button _btnPrev;
    private readonly Button _btnNext;
    private readonly System.Windows.Forms.Timer _timer;

    public ExamControl(Exam exam, Subject subject, Student student,
        Repository<Exam> history, Action<Exam, Subject> onFinished)
    {
        _exam = exam;
        _subject = subject;
        _student = student;
        _history = history;
        _onFinished = onFinished;
        _secondsLeft = exam.Time * 60;

        Dock = DockStyle.Fill;
        BackColor = Theme.BgDark;
        Padding = new Padding(40, 24, 40, 24);

        // Top bar
        var topBar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.Transparent };

        string examType = exam is PracticeExam ? "Practice" : "Final";
        _lblTitle = Theme.MakeLabel($"{subject.Name} — {examType} Exam", Theme.FontSubtitle, Theme.Accent);
        _lblTitle.Location = new Point(0, 8);
        topBar.Controls.Add(_lblTitle);

        _lblTimer = Theme.MakeLabel(FormatTime(_secondsLeft), Theme.FontTimer, Theme.Warning);
        _lblTimer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _lblTimer.Location = new Point(topBar.Width - 160, 8);
        topBar.Controls.Add(_lblTimer);
        topBar.Resize += (_, _) => _lblTimer.Left = topBar.Width - _lblTimer.Width - 4;

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Bottom,
            Height = 6,
            Maximum = exam.Questions.Length,
            Style = ProgressBarStyle.Continuous,
            ForeColor = Theme.Accent,
            BackColor = Theme.BgCard,
        };
        topBar.Controls.Add(_progressBar);

        _lblProgress = Theme.MakeLabel($"Question 1 of {exam.Questions.Length}", Theme.FontSmall, Theme.TextMuted);
        _lblProgress.Location = new Point(0, 42);
        topBar.Controls.Add(_lblProgress);

        Controls.Add(topBar);

        // Question area
        _questionPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, AutoScroll = true };
        Controls.Add(_questionPanel);

        // Bottom bar
        var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.Transparent };
        _btnPrev = Theme.MakeButton("←  Previous", Color.FromArgb(55, 55, 85), 150, 42);
        _btnPrev.Dock = DockStyle.Left;
        _btnPrev.Click += (_, _) => Navigate(-1);

        _btnNext = Theme.MakeButton("Next  →", null, 150, 42);
        _btnNext.Dock = DockStyle.Right;
        _btnNext.Click += (_, _) => Navigate(1);

        bottomBar.Controls.Add(_btnPrev);
        bottomBar.Controls.Add(_btnNext);
        Controls.Add(bottomBar);

        // Timer
        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        ShowQuestion();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _secondsLeft--;
        _lblTimer.Text = FormatTime(_secondsLeft);
        if (_secondsLeft <= 60)
            _lblTimer.ForeColor = Theme.Danger;
        if (_secondsLeft <= 0)
        {
            _timer.Stop();
            FinishExam();
        }
    }

    private static string FormatTime(int totalSeconds)
    {
        int m = Math.Max(0, totalSeconds) / 60;
        int s = Math.Max(0, totalSeconds) % 60;
        return $"{m:D2}:{s:D2}";
    }

    private void Navigate(int dir)
    {
        SaveCurrentAnswer();
        int next = _currentIndex + dir;
        if (next < 0 || next >= _exam.Questions.Length)
        {
            if (dir > 0) FinishExam();
            return;
        }
        _currentIndex = next;
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        _questionPanel.SuspendLayout();
        _questionPanel.Controls.Clear();

        var q = _exam.Questions[_currentIndex];
        _progressBar.Value = _currentIndex + 1;
        _lblProgress.Text = $"Question {_currentIndex + 1} of {_exam.Questions.Length}";
        _btnPrev.Enabled = _currentIndex > 0;
        _btnNext.Text = _currentIndex == _exam.Questions.Length - 1 ? "Finish  ✓" : "Next  →";

        int y = 16;

        var card = new Panel
        {
            Location = new Point(0, y),
            Width = Math.Max(200, _questionPanel.Width - 20),
            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Theme.BgCard,
            Padding = new Padding(28),
        };

        int cy = 20;

        var lblHeader = new Label
        {
            Text = q.Header,
            Font = Theme.FontSmall,
            ForeColor = Theme.Accent,
            Location = new Point(28, cy),
            AutoSize = true,
            BackColor = Theme.BgCard,
        };
        card.Controls.Add(lblHeader);
        cy += 28;

        var lblBody = new Label
        {
            Text = q.Body,
            Font = Theme.FontSubtitle,
            ForeColor = Theme.TextPrimary,
            Location = new Point(28, cy),
            MaximumSize = new Size(Math.Max(100, card.Width - 60), 0),
            AutoSize = true,
            BackColor = Theme.BgCard,
        };
        card.Controls.Add(lblBody);
        cy += lblBody.GetPreferredSize(new Size(Math.Max(100, card.Width - 60), 0)).Height + 12;

        var lblMarks = new Label
        {
            Text = $"{q.Marks} mark{(q.Marks > 1 ? "s" : "")}",
            Font = Theme.FontSmall,
            ForeColor = Theme.TextMuted,
            Location = new Point(28, cy),
            AutoSize = true,
            BackColor = Theme.BgCard,
        };
        card.Controls.Add(lblMarks);
        cy += 36;

        bool isMulti = q is ChooseAllQuestion;

        for (int i = 0; i < q.Answers.Count; i++)
        {
            var ans = q.Answers[i];
            int ansWidth = Math.Max(100, card.Width - 60);

            if (isMulti)
            {
                var chk = new CheckBox
                {
                    Text = "  " + ans.Text,
                    Font = Theme.FontBody,
                    ForeColor = Theme.TextPrimary,
                    Location = new Point(28, cy),
                    Width = ansWidth,
                    Height = 44,
                    Appearance = Appearance.Button,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.BgInput,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 0, 0),
                    Tag = i,
                };
                chk.FlatAppearance.BorderSize = 1;
                chk.FlatAppearance.BorderColor = Theme.Border;
                chk.FlatAppearance.CheckedBackColor = Color.FromArgb(60, Theme.Accent.R, Theme.Accent.G, Theme.Accent.B);

                if (_answers.TryGetValue(_currentIndex, out var prev))
                {
                    var ids = prev.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    chk.Checked = ids.Contains(ans.Id.ToString());
                }

                card.Controls.Add(chk);
            }
            else
            {
                var rb = new RadioButton
                {
                    Text = "  " + ans.Text,
                    Font = Theme.FontBody,
                    ForeColor = Theme.TextPrimary,
                    Location = new Point(28, cy),
                    Width = ansWidth,
                    Height = 44,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.BgInput,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 0, 0),
                    Tag = i,
                };

                if (_answers.TryGetValue(_currentIndex, out var prev))
                {
                    rb.Checked = prev.Text == ans.Id.ToString();
                }

                card.Controls.Add(rb);
            }

            cy += 50;
        }

        card.Height = cy + 20;
        _questionPanel.Controls.Add(card);
        _questionPanel.ResumeLayout(true);
    }

    private void SaveCurrentAnswer()
    {
        if (_questionPanel.Controls.Count == 0) return;
        var card = _questionPanel.Controls[0];
        var q = _exam.Questions[_currentIndex];
        bool isMulti = q is ChooseAllQuestion;

        if (isMulti)
        {
            var ids = new List<string>();
            foreach (Control c in card.Controls)
            {
                if (c is CheckBox chk && chk.Checked)
                {
                    int idx = (int)chk.Tag;
                    ids.Add(q.Answers[idx].Id.ToString());
                }
            }
            if (ids.Count > 0)
                _answers[_currentIndex] = new Answer(string.Join(",", ids));
        }
        else
        {
            foreach (Control c in card.Controls)
            {
                if (c is RadioButton rb && rb.Checked)
                {
                    int idx = (int)rb.Tag;
                    _answers[_currentIndex] = new Answer(q.Answers[idx].Id.ToString());
                }
            }
        }
    }

    private void FinishExam()
    {
        _timer.Stop();
        SaveCurrentAnswer();

        // copy answers to exam
        _exam.QuestionAnswerDictionary.Clear();
        for (int i = 0; i < _exam.Questions.Length; i++)
        {
            if (_answers.TryGetValue(i, out var a))
                _exam.QuestionAnswerDictionary[_exam.Questions[i]] = a;
            else
                _exam.QuestionAnswerDictionary[_exam.Questions[i]] = new Answer("");
        }

        // Set mode directly — do NOT call _exam.Finish() because it uses Console.Clear()
        _exam.Mode = ExamMode.Finished;
        ResultLogger.LogExamResult(_student, _subject, _exam);

        var clone = (Exam)_exam.Clone();
        foreach (var kvp in _exam.QuestionAnswerDictionary)
            clone.QuestionAnswerDictionary[kvp.Key] = kvp.Value;
        _history.Add(clone);

        _onFinished(_exam, _subject);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}
