namespace ExamApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlSidebar = new Panel();
            lblLogo = new Label();
            pnlNavButtons = new FlowLayoutPanel();
            lblUserInfo = new Label();
            btnLogout = new Button();
            pnlContent = new Panel();

            SuspendLayout();

            // pnlSidebar
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Width = 220;
            pnlSidebar.BackColor = Theme.BgSidebar;
            pnlSidebar.Padding = new Padding(0, 16, 0, 16);

            // lblLogo
            lblLogo.Dock = DockStyle.Top;
            lblLogo.Height = 72;
            lblLogo.Text = "  📝 ExamSys";
            lblLogo.Font = Theme.FontSubtitle;
            lblLogo.ForeColor = Theme.Accent;
            lblLogo.TextAlign = ContentAlignment.MiddleLeft;
            lblLogo.Padding = new Padding(14, 0, 0, 0);

            // pnlNavButtons
            pnlNavButtons.Dock = DockStyle.Fill;
            pnlNavButtons.FlowDirection = FlowDirection.TopDown;
            pnlNavButtons.WrapContents = false;
            pnlNavButtons.AutoScroll = true;
            pnlNavButtons.Padding = new Padding(10, 8, 10, 0);
            pnlNavButtons.BackColor = Color.Transparent;

            // lblUserInfo
            lblUserInfo.Dock = DockStyle.Bottom;
            lblUserInfo.Height = 44;
            lblUserInfo.Font = Theme.FontSmall;
            lblUserInfo.ForeColor = Theme.TextMuted;
            lblUserInfo.TextAlign = ContentAlignment.MiddleCenter;
            lblUserInfo.Text = "";

            // btnLogout
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.Height = 42;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.BackColor = Color.FromArgb(60, 248, 113, 113);
            btnLogout.ForeColor = Theme.Danger;
            btnLogout.Font = Theme.FontNav;
            btnLogout.Text = "⏻  Logout";
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.Visible = false;

            pnlSidebar.Controls.Add(pnlNavButtons);
            pnlSidebar.Controls.Add(lblUserInfo);
            pnlSidebar.Controls.Add(btnLogout);
            pnlSidebar.Controls.Add(lblLogo);

            // pnlContent
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.BackColor = Theme.BgDark;
            pnlContent.Padding = new Padding(0);

            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);

            // Form1
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 680);
            MinimumSize = new Size(960, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Examination Management System";
            Name = "Form1";

            ResumeLayout(false);
        }

        #endregion

        private Panel pnlSidebar;
        private Label lblLogo;
        private FlowLayoutPanel pnlNavButtons;
        private Label lblUserInfo;
        private Button btnLogout;
        private Panel pnlContent;
    }
}
