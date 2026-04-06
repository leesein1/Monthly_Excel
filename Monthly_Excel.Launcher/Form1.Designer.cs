namespace Monthly_Excel.Launcher;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private Label titleLabel;
    private Label statusLabel;
    private ProgressBar progressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        titleLabel = new Label();
        statusLabel = new Label();
        progressBar = new ProgressBar();
        SuspendLayout();
        // 
        // titleLabel
        // 
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
        titleLabel.ForeColor = Color.FromArgb(33, 43, 54);
        titleLabel.Location = new Point(20, 20);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(320, 34);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "Monthly Excel";
        titleLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // statusLabel
        // 
        statusLabel.Dock = DockStyle.Top;
        statusLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        statusLabel.ForeColor = Color.FromArgb(95, 108, 122);
        statusLabel.Location = new Point(20, 54);
        statusLabel.Name = "statusLabel";
        statusLabel.Padding = new Padding(0, 8, 0, 8);
        statusLabel.Size = new Size(320, 42);
        statusLabel.TabIndex = 1;
        statusLabel.Text = "초기화 중...";
        statusLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // progressBar
        // 
        progressBar.Dock = DockStyle.Top;
        progressBar.Location = new Point(20, 96);
        progressBar.MarqueeAnimationSpeed = 30;
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(320, 14);
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.TabIndex = 2;
        // 
        // Form1
        // 
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(360, 140);
        Controls.Add(progressBar);
        Controls.Add(statusLabel);
        Controls.Add(titleLabel);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "Form1";
        Padding = new Padding(20);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Monthly Excel Launcher";
        TopMost = true;
        ResumeLayout(false);
    }
}
