using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ExamApp;

public static class Theme
{
    // ── Core palette ──
    public static readonly Color BgDark = Color.FromArgb(22, 22, 36);
    public static readonly Color BgMedium = Color.FromArgb(30, 30, 50);
    public static readonly Color BgCard = Color.FromArgb(38, 38, 62);
    public static readonly Color BgInput = Color.FromArgb(46, 46, 72);
    public static readonly Color BgSidebar = Color.FromArgb(18, 18, 32);

    public static readonly Color Accent = Color.FromArgb(99, 102, 241);
    public static readonly Color AccentHover = Color.FromArgb(129, 132, 255);
    public static readonly Color AccentDim = Color.FromArgb(60, 62, 140);
    public static readonly Color Success = Color.FromArgb(52, 211, 153);
    public static readonly Color Danger = Color.FromArgb(248, 113, 113);
    public static readonly Color Warning = Color.FromArgb(251, 191, 36);
    public static readonly Color Info = Color.FromArgb(96, 165, 250);

    public static readonly Color TextPrimary = Color.FromArgb(237, 237, 245);
    public static readonly Color TextSecondary = Color.FromArgb(160, 160, 185);
    public static readonly Color TextMuted = Color.FromArgb(100, 100, 130);
    public static readonly Color Border = Color.FromArgb(55, 55, 85);

    // ── Fonts ──
    public static readonly Font FontTitle = new("Segoe UI Semibold", 22F);
    public static readonly Font FontSubtitle = new("Segoe UI Semibold", 15F);
    public static readonly Font FontBody = new("Segoe UI", 11F);
    public static readonly Font FontBodyBold = new("Segoe UI Semibold", 11F);
    public static readonly Font FontSmall = new("Segoe UI", 9.5F);
    public static readonly Font FontNav = new("Segoe UI Semibold", 12F);
    public static readonly Font FontButton = new("Segoe UI Semibold", 11F);
    public static readonly Font FontBig = new("Segoe UI Semibold", 28F);
    public static readonly Font FontTimer = new("Cascadia Code", 18F);

    // ── Control factories ──

    public static Label MakeLabel(string text, Font? font = null, Color? color = null)
    {
        return new Label
        {
            Text = text,
            Font = font ?? FontBody,
            ForeColor = color ?? TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = true,
        };
    }

    public static TextBox MakeTextBox(int width = 300, bool password = false)
    {
        var tb = new TextBox
        {
            Width = width,
            Height = 38,
            Font = FontBody,
            ForeColor = TextPrimary,
            BackColor = BgInput,
            BorderStyle = BorderStyle.FixedSingle,
        };
        if (password) tb.UseSystemPasswordChar = true;
        return tb;
    }

    public static Button MakeButton(string text, Color? bg = null, int width = 200, int height = 42)
    {
        var btn = new Button
        {
            Text = text,
            Font = FontButton,
            Width = width,
            Height = height,
            FlatStyle = FlatStyle.Flat,
            BackColor = bg ?? Accent,
            ForeColor = TextPrimary,
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter,
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = AccentHover;
        btn.FlatAppearance.MouseDownBackColor = AccentDim;
        return btn;
    }

    public static Panel MakeCard(int width = 400, int height = 0, DockStyle dock = DockStyle.None)
    {
        var p = new RoundedPanel
        {
            Width = width,
            BackColor = BgCard,
            Padding = new Padding(24),
        };
        if (height > 0) p.Height = height;
        if (dock != DockStyle.None) p.Dock = dock;
        return p;
    }

    public static DataGridView MakeGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = BgMedium,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Border,
            Font = FontBody,
            ForeColor = TextPrimary,
            DefaultCellStyle =
            {
                BackColor = BgMedium,
                ForeColor = TextPrimary,
                SelectionBackColor = AccentDim,
                SelectionForeColor = TextPrimary,
                Padding = new Padding(6, 4, 6, 4),
            },
            ColumnHeadersDefaultCellStyle =
            {
                BackColor = BgCard,
                ForeColor = Accent,
                Font = FontBodyBold,
                Padding = new Padding(6, 6, 6, 6),
            },
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = 40 },
            ColumnHeadersHeight = 44,
        };
        return grid;
    }

    public static void StyleComboBox(ComboBox cb)
    {
        cb.Font = FontBody;
        cb.ForeColor = TextPrimary;
        cb.BackColor = BgInput;
        cb.FlatStyle = FlatStyle.Flat;
        cb.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    public static void ApplyToForm(Form form)
    {
        form.BackColor = BgDark;
        form.ForeColor = TextPrimary;
        form.Font = FontBody;
    }

    public static LinearGradientBrush GradientBrush(Rectangle rect, Color c1, Color c2)
    {
        if (rect.Width < 1) rect.Width = 1;
        if (rect.Height < 1) rect.Height = 1;
        return new LinearGradientBrush(rect, c1, c2, 135F);
    }
}

public class RoundedPanel : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Radius { get; set; } = 16;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        ApplyRegion();
    }

    private void ApplyRegion()
    {
        if (Width < 1 || Height < 1) return;
        var path = GetRoundedRect(new Rectangle(0, 0, Width, Height), Radius);
        Region = new Region(path);
        path.Dispose();
    }

    private static GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
