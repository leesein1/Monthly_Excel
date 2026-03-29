using System.Drawing;
using System.Windows.Forms;

namespace Monthly_Excel.UI
{
    internal static class AppTheme
    {
        public static readonly Color AppBackground = Color.FromArgb(245, 247, 250);
        public static readonly Color SurfaceBackground = Color.White;
        public static readonly Color SurfaceMuted = Color.FromArgb(236, 241, 246);
        public static readonly Color Accent = Color.FromArgb(43, 122, 120);
        public static readonly Color AccentStrong = Color.FromArgb(31, 92, 91);
        public static readonly Color Border = Color.FromArgb(212, 220, 228);
        public static readonly Color TextPrimary = Color.FromArgb(33, 43, 54);
        public static readonly Color TextMuted = Color.FromArgb(95, 108, 122);

        public static readonly Padding SectionPadding = new(16);
        public static readonly Padding ControlMargin = new(0, 0, 0, 8);
        public static readonly Font TitleFont = new("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font BodyFont = new("Segoe UI", 9F, FontStyle.Regular);

        public static void ApplyPage(UserControl page)
        {
            page.BackColor = AppBackground;
            page.Font = BodyFont;
        }

        public static Panel CreateSurfacePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceBackground,
                Padding = SectionPadding
            };
        }

        public static void StylePrimaryButton(Button button)
        {
            button.AutoSize = false;
            button.Height = 28;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Accent;
            button.ForeColor = Color.White;
            button.Font = BodyFont;
        }

        public static void StyleSecondaryButton(Button button)
        {
            button.AutoSize = false;
            button.Height = 28;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.BorderSize = 1;
            button.BackColor = SurfaceBackground;
            button.ForeColor = TextPrimary;
            button.Font = BodyFont;
        }

        public static void StyleTextBox(TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            textBox.ForeColor = TextPrimary;
            textBox.Font = BodyFont;
        }

        public static void StyleListBox(ListBox listBox)
        {
            listBox.BorderStyle = BorderStyle.FixedSingle;
            listBox.BackColor = SurfaceBackground;
            listBox.ForeColor = TextPrimary;
            listBox.Font = BodyFont;
            listBox.ItemHeight = 20;
        }

        public static void StyleStatusLabel(Label label)
        {
            label.ForeColor = TextMuted;
            label.Font = BodyFont;
        }

        public static void StyleSectionLabel(Label label)
        {
            label.ForeColor = TextPrimary;
            label.Font = TitleFont;
        }

        public static void StyleProgressBar(ProgressBar progressBar)
        {
            progressBar.ForeColor = Accent;
        }
    }
}
