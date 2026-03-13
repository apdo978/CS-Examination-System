using ConsoleApp2;

namespace ExamApp.Controls;

public class AuthControl : UserControl
{
    private readonly Action<UserAccount> _onLogin;
    private bool _isLoginMode = true;

    // card container
    private readonly Panel _card;

    // controls
    private readonly Label _title;
    private readonly Label _lblName;
    private readonly TextBox _txtName;
    private readonly Label _lblEmail;
    private readonly TextBox _txtEmail;
    private readonly Label _lblPassword;
    private readonly TextBox _txtPassword;
    private readonly Label _lblConfirm;
    private readonly TextBox _txtConfirm;
    private readonly Label _lblRole;
    private readonly ComboBox _cmbRole;
    private readonly Button _btnSubmit;
    private readonly Label _lblError;
    private readonly Label _toggleLabel;
    private readonly LinkLabel _toggleLink;

    private const int FieldWidth = 320;
    private const int CardWidth = 400;

    public AuthControl(Action<UserAccount> onLogin)
    {
        _onLogin = onLogin;
        Dock = DockStyle.Fill;
        BackColor = Theme.BgDark;
        DoubleBuffered = true;

        // Build a fixed-size card (no AutoSize — we manage height manually)
        _card = new Panel
        {
            Width = CardWidth,
            BackColor = Theme.BgCard,
            Padding = new Padding(40, 32, 40, 24),
        };

        int left = 40;
        int y = 32;

        // Title
        _title = new Label
        {
            Text = "Sign In",
            Font = Theme.FontTitle,
            ForeColor = Theme.Accent,
            BackColor = Color.Transparent,
            Location = new Point(left, y),
            AutoSize = true,
        };
        _card.Controls.Add(_title);
        y += 48;

        // Name (hidden in login mode)
        _lblName = MakeFieldLabel("Full Name", left, y);
        _card.Controls.Add(_lblName);
        y += 22;
        _txtName = Theme.MakeTextBox(FieldWidth);
        _txtName.Location = new Point(left, y);
        _card.Controls.Add(_txtName);
        y += 44;
        _lblName.Visible = false;
        _txtName.Visible = false;

        // Email
        _lblEmail = MakeFieldLabel("Email", left, y);
        _card.Controls.Add(_lblEmail);
        y += 22;
        _txtEmail = Theme.MakeTextBox(FieldWidth);
        _txtEmail.Location = new Point(left, y);
        _card.Controls.Add(_txtEmail);
        y += 44;

        // Password
        _lblPassword = MakeFieldLabel("Password", left, y);
        _card.Controls.Add(_lblPassword);
        y += 22;
        _txtPassword = Theme.MakeTextBox(FieldWidth, password: true);
        _txtPassword.Location = new Point(left, y);
        _card.Controls.Add(_txtPassword);
        y += 44;

        // Confirm (hidden in login mode)
        _lblConfirm = MakeFieldLabel("Confirm Password", left, y);
        _card.Controls.Add(_lblConfirm);
        y += 22;
        _txtConfirm = Theme.MakeTextBox(FieldWidth, password: true);
        _txtConfirm.Location = new Point(left, y);
        _card.Controls.Add(_txtConfirm);
        y += 44;
        _lblConfirm.Visible = false;
        _txtConfirm.Visible = false;

        // Role (hidden in login mode)
        _lblRole = MakeFieldLabel("Role", left, y);
        _card.Controls.Add(_lblRole);
        y += 22;
        _cmbRole = new ComboBox { Width = FieldWidth, Location = new Point(left, y) };
        Theme.StyleComboBox(_cmbRole);
        _cmbRole.Items.AddRange(["Student", "Teacher"]);
        _cmbRole.SelectedIndex = 0;
        _card.Controls.Add(_cmbRole);
        y += 40;
        _lblRole.Visible = false;
        _cmbRole.Visible = false;

        // Error label
        _lblError = new Label
        {
            Text = "",
            Font = Theme.FontSmall,
            ForeColor = Theme.Danger,
            BackColor = Color.Transparent,
            Location = new Point(left, y),
            MaximumSize = new Size(FieldWidth, 0),
            AutoSize = true,
        };
        _card.Controls.Add(_lblError);
        y += 8;

        // Submit button
        _btnSubmit = Theme.MakeButton("Sign In", width: FieldWidth, height: 44);
        _btnSubmit.Location = new Point(left, y);
        _btnSubmit.Click += OnSubmit;
        _card.Controls.Add(_btnSubmit);
        y += 56;

        // Toggle link
        _toggleLabel = new Label
        {
            Text = "Don't have an account?",
            Font = Theme.FontSmall,
            ForeColor = Theme.TextMuted,
            BackColor = Color.Transparent,
            Location = new Point(left, y),
            AutoSize = true,
        };
        _card.Controls.Add(_toggleLabel);

        _toggleLink = new LinkLabel
        {
            Text = "Create one",
            Font = Theme.FontSmall,
            LinkColor = Theme.Accent,
            ActiveLinkColor = Theme.AccentHover,
            BackColor = Color.Transparent,
            AutoSize = true,
        };
        _toggleLink.Click += (_, _) => ToggleMode();
        _card.Controls.Add(_toggleLink);
        y += 28;

        _card.Height = y;

        // Keyboard shortcuts
        _txtPassword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter && _isLoginMode) OnSubmit(this, EventArgs.Empty); };
        _txtConfirm.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter && !_isLoginMode) OnSubmit(this, EventArgs.Empty); };

        Controls.Add(_card);
        Resize += (_, _) => LayoutCard();
    }

    private void LayoutCard()
    {
        _card.Left = Math.Max(0, (Width - _card.Width) / 2);
        _card.Top = Math.Max(12, (Height - _card.Height) / 2);

        // Position the toggle link beside the label
        _toggleLink.Location = new Point(_toggleLabel.Right + 6, _toggleLabel.Top);
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible) LayoutCard();
    }

    private void ToggleMode()
    {
        _isLoginMode = !_isLoginMode;
        _lblError.Text = "";

        bool reg = !_isLoginMode;
        _title.Text = reg ? "Create Account" : "Sign In";
        _lblName.Visible = reg;
        _txtName.Visible = reg;
        _lblConfirm.Visible = reg;
        _txtConfirm.Visible = reg;
        _lblRole.Visible = reg;
        _cmbRole.Visible = reg;
        _btnSubmit.Text = reg ? "Register" : "Sign In";
        _toggleLabel.Text = reg ? "Already have an account?" : "Don't have an account?";
        _toggleLink.Text = reg ? "Sign in" : "Create one";

        // Recalculate positions
        int left = 40;
        int y = 32 + 48; // after title

        if (reg)
        {
            _lblName.Location = new Point(left, y); y += 22;
            _txtName.Location = new Point(left, y); y += 44;
        }

        _lblEmail.Location = new Point(left, y); y += 22;
        _txtEmail.Location = new Point(left, y); y += 44;

        _lblPassword.Location = new Point(left, y); y += 22;
        _txtPassword.Location = new Point(left, y); y += 44;

        if (reg)
        {
            _lblConfirm.Location = new Point(left, y); y += 22;
            _txtConfirm.Location = new Point(left, y); y += 44;
            _lblRole.Location = new Point(left, y); y += 22;
            _cmbRole.Location = new Point(left, y); y += 40;
        }

        _lblError.Location = new Point(left, y); y += 8;
        _btnSubmit.Location = new Point(left, y); y += 56;
        _toggleLabel.Location = new Point(left, y);
        y += 28;

        _card.Height = y;
        LayoutCard();
    }

    private void OnSubmit(object? sender, EventArgs e)
    {
        _lblError.Text = "";

        string email = _txtEmail.Text.Trim();
        string password = _txtPassword.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _lblError.Text = "Email and password are required.";
            return;
        }

        if (!IsValidEmail(email))
        {
            _lblError.Text = "Please enter a valid email address (e.g. user@example.com).";
            return;
        }

        if (_isLoginMode)
        {
            var account = AuthService.Login(email, password);
            if (account == null)
            {
                _lblError.Text = "Invalid email or password.";
                return;
            }
            _onLogin(account);
        }
        else
        {
            string name = _txtName.Text.Trim();
            string confirm = _txtConfirm.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                _lblError.Text = "Name is required.";
                return;
            }
            if (password.Length < 4)
            {
                _lblError.Text = "Password must be at least 4 characters.";
                return;
            }
            if (password != confirm)
            {
                _lblError.Text = "Passwords do not match.";
                return;
            }

            var role = _cmbRole.SelectedIndex == 0 ? UserRole.Student : UserRole.Teacher;
            bool ok = AuthService.Register(email, password, name, role);

            if (!ok)
            {
                _lblError.Text = "Registration failed. Email may already be registered.";
                return;
            }

            var account = AuthService.Login(email, password);
            if (account != null) _onLogin(account);
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        int at = email.IndexOf('@');
        int dot = email.LastIndexOf('.');
        return at > 0 && dot > at + 1 && dot < email.Length - 1;
    }

    private static Label MakeFieldLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Font = Theme.FontSmall,
            ForeColor = Theme.TextSecondary,
            BackColor = Color.Transparent,
            Location = new Point(x, y),
            AutoSize = true,
        };
    }
}
