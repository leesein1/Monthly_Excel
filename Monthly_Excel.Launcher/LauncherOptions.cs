using System.Diagnostics;

namespace Monthly_Excel.Launcher;

internal sealed class LauncherOptions
{
    private const string MainExecutableName = "Monthly_Excel.exe";

    private LauncherOptions(string? manifestUrl)
    {
        ManifestUrl = manifestUrl;
    }

    public string? ManifestUrl { get; }

    public static LauncherOptions CreateDefault()
    {
        var configuredUrl = Environment.GetEnvironmentVariable("MONTHLY_EXCEL_MANIFEST_URL");
        var fallbackUrl = LauncherConfiguration.ManifestUrl;
        var selectedUrl = string.IsNullOrWhiteSpace(configuredUrl) ? fallbackUrl : configuredUrl.Trim();
        return new LauncherOptions(string.IsNullOrWhiteSpace(selectedUrl) ? null : selectedUrl.Trim());
    }

    public string ResolveMainExecutablePath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, MainExecutableName),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Monthly_Excel", "bin", "Debug", "net8.0-windows", MainExecutableName)),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Monthly_Excel", "bin", "Release", "net8.0-windows", MainExecutableName))
        };

        return candidates.FirstOrDefault(File.Exists)
            ?? Path.Combine(AppContext.BaseDirectory, MainExecutableName);
    }

    public string ResolveApplicationRoot()
    {
        return Path.GetDirectoryName(ResolveMainExecutablePath()) ?? AppContext.BaseDirectory;
    }

    public string GetStateDirectory()
    {
        var stateDirectory = Path.Combine(ResolveApplicationRoot(), ".launcher");
        Directory.CreateDirectory(stateDirectory);
        return stateDirectory;
    }

    public string GetCurrentManifestPath() => Path.Combine(GetStateDirectory(), "current-manifest.json");

    public bool CanCheckForUpdates() => !string.IsNullOrWhiteSpace(ManifestUrl);

    public bool IsLauncherManagedFile(string relativePath)
    {
        var normalized = NormalizeRelativePath(relativePath);
        return normalized.Equals("Monthly_Excel.Launcher.exe", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Monthly_Excel.Launcher.dll", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith(".launcher/", StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeRelativePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }

    public ProcessStartInfo CreateLaunchStartInfo()
    {
        var executablePath = ResolveMainExecutablePath();
        return new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = Path.GetDirectoryName(executablePath) ?? AppContext.BaseDirectory,
            UseShellExecute = true
        };
    }
}
