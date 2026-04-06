namespace Monthly_Excel.Launcher;

internal sealed record LauncherRunResult(bool Succeeded, string Message)
{
    public static LauncherRunResult Success(string message) => new(true, message);

    public static LauncherRunResult Failure(string message) => new(false, message);
}
