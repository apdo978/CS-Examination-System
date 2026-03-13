using ConsoleApp2;

namespace ExamApp;

public partial class Form1 : Form
{
    private readonly Subject[] _subjects;
    private UserAccount? _currentUser;
    private Student? _currentStudent;
    private readonly Repository<Exam> _sessionHistory = new();

    private Button? _activeNavButton;

    public Form1()
    {
        InitializeComponent();
        Theme.ApplyToForm(this);
        _subjects = DataSeeder.BuildSubjects();

        btnLogout.Click += (_, _) => Logout();

        ShowView(new Controls.AuthControl(OnLoginSuccess));
    }

    // ── Navigation ──

    private void OnLoginSuccess(UserAccount account)
    {
        _currentUser = account;
        lblUserInfo.Text = $"{account.Name}\n{account.Role}";
        btnLogout.Visible = true;

        if (account.Role == UserRole.Student)
        {
            _currentStudent = new Student(account.Name, account.Email);
            foreach (var s in _subjects) s.Enroll(_currentStudent);
            BuildSidebar(
                ("🏠  Dashboard", () => ShowStudentDash()),
                ("📊  My Results", () => ShowStudentResults())
            );
            ShowStudentDash();
        }
        else
        {
            BuildSidebar(
                ("👥  Students", () => ShowTeacherDash(0)),
                ("📋  Results", () => ShowTeacherDash(1)),
                ("📈  Summary", () => ShowTeacherDash(2))
            );
            ShowTeacherDash(0);
        }
    }

    private void Logout()
    {
        _currentUser = null;
        _currentStudent = null;
        lblUserInfo.Text = "";
        btnLogout.Visible = false;
        pnlNavButtons.Controls.Clear();
        ShowView(new Controls.AuthControl(OnLoginSuccess));
    }

    private void BuildSidebar(params (string text, Action action)[] items)
    {
        pnlNavButtons.Controls.Clear();
        _activeNavButton = null;

        foreach (var (text, action) in items)
        {
            var btn = new Button
            {
                Text = text,
                Font = Theme.FontNav,
                ForeColor = Theme.TextSecondary,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Width = 196,
                Height = 44,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 2),
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 99, 102, 241);

            btn.Click += (_, _) =>
            {
                SetActiveNav(btn);
                action();
            };

            pnlNavButtons.Controls.Add(btn);
        }

        if (pnlNavButtons.Controls.Count > 0)
            SetActiveNav((Button)pnlNavButtons.Controls[0]);
    }

    private void SetActiveNav(Button btn)
    {
        if (_activeNavButton != null)
        {
            _activeNavButton.BackColor = Color.Transparent;
            _activeNavButton.ForeColor = Theme.TextSecondary;
        }
        _activeNavButton = btn;
        btn.BackColor = Color.FromArgb(50, 99, 102, 241);
        btn.ForeColor = Theme.Accent;
    }

    // ── View switching ──

    public void ShowView(Control view)
    {
        pnlContent.SuspendLayout();
        pnlContent.Controls.Clear();
        view.Dock = DockStyle.Fill;
        pnlContent.Controls.Add(view);
        pnlContent.ResumeLayout(true);
    }

    private void ShowStudentDash()
    {
        ShowView(new Controls.StudentDashControl(_subjects, StartExam));
    }

    private void ShowStudentResults()
    {
        if (_currentUser == null) return;
        var grades = ResultLogger.GetGradesForStudent(_currentUser.Email);
        ShowView(new Controls.ExamResultControl(grades, _currentUser.Name));
    }

    private void StartExam(Subject subject, bool isPractice)
    {
        if (_currentStudent == null) return;
        var exam = isPractice ? (Exam?)subject.PracticalExam : subject.FinalExam;
        if (exam == null) return;

        ShowView(new Controls.ExamControl(exam, subject, _currentStudent, _sessionHistory, OnExamFinished));
    }

    private void OnExamFinished(Exam exam, Subject subject)
    {
        if (_currentUser == null) return;
        var grades = ResultLogger.GetGradesForStudent(_currentUser.Email);
        ShowView(new Controls.ExamResultControl(grades, _currentUser.Name, exam, subject));
    }

    private void ShowTeacherDash(int tabIndex)
    {
        ShowView(new Controls.TeacherDashControl(tabIndex));
    }
}

