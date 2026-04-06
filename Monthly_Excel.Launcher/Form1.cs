namespace Monthly_Excel.Launcher;

public partial class Form1 : Form
{
    private readonly LauncherOptions _options = LauncherOptions.CreateDefault();
    private readonly GitHubReleaseUpdateService _updateService;

    public Form1()
    {
        InitializeComponent();
        _updateService = new GitHubReleaseUpdateService(_options);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (DesignMode)
        {
            return;
        }

        await RunLaunchFlowAsync();
    }

    private async Task RunLaunchFlowAsync()
    {
        try
        {
            SetStatus("업데이트 확인 중...");
            SetProgressStyle(ProgressBarStyle.Marquee);

            var result = await _updateService.RunAsync(new Progress<LauncherProgress>(ReportProgress));

            if (!result.Succeeded)
            {
                MessageBox.Show(
                    result.Message,
                    "실행 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
                return;
            }

            SetStatus("프로그램 실행 중...");
            await Task.Delay(200);
            _updateService.LaunchMainApplication();
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"런처 실행 중 오류가 발생했습니다.\r\n{ex.Message}",
                "실행 오류",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            Close();
        }
    }

    private void ReportProgress(LauncherProgress progress)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => ReportProgress(progress));
            return;
        }

        SetStatus(progress.Message);

        if (progress.Percent.HasValue)
        {
            SetProgressStyle(ProgressBarStyle.Continuous);
            progressBar.Value = Math.Max(progressBar.Minimum, Math.Min(progressBar.Maximum, progress.Percent.Value));
        }
        else
        {
            SetProgressStyle(ProgressBarStyle.Marquee);
        }
    }

    private void SetStatus(string message)
    {
        statusLabel.Text = message;
    }

    private void SetProgressStyle(ProgressBarStyle style)
    {
        if (progressBar.Style == style)
        {
            return;
        }

        progressBar.Style = style;
        if (style == ProgressBarStyle.Continuous)
        {
            progressBar.Value = 0;
        }
    }
}
