using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace Monthly_Excel.Launcher;

internal sealed class GitHubReleaseUpdateService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private static readonly HttpClient SharedHttpClient = CreateHttpClient();
    private readonly LauncherOptions _options;

    public GitHubReleaseUpdateService(LauncherOptions options)
    {
        _options = options;
    }

    public async Task<LauncherRunResult> RunAsync(IProgress<LauncherProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var executablePath = _options.ResolveMainExecutablePath();
        if (!File.Exists(executablePath))
        {
            return LauncherRunResult.Failure($"본 프로그램을 찾을 수 없습니다.\r\n{executablePath}");
        }

        if (!_options.CanCheckForUpdates())
        {
            progress?.Report(new LauncherProgress("업데이트 주소가 아직 설정되지 않아 바로 실행합니다.", 100));
            return LauncherRunResult.Success("업데이트 생략");
        }

        progress?.Report(new LauncherProgress("릴리즈 정보 확인 중..."));

        ReleaseManifest remoteManifest;
        try
        {
            remoteManifest = await DownloadRemoteManifestAsync(cancellationToken);
        }
        catch
        {
            progress?.Report(new LauncherProgress("업데이트 서버에 연결하지 못해 기존 버전으로 실행합니다.", 100));
            return LauncherRunResult.Success("오프라인 실행");
        }

        var changeSet = await BuildChangeSetAsync(remoteManifest, cancellationToken);
        if (changeSet.FilesToDownload.Count == 0 && changeSet.FilesToDelete.Count == 0)
        {
            progress?.Report(new LauncherProgress("변경 사항이 없습니다.", 100));
            await SaveInstalledManifestAsync(remoteManifest, cancellationToken);
            return LauncherRunResult.Success("최신 버전");
        }

        await ApplyChangesAsync(remoteManifest, changeSet, progress, cancellationToken);
        progress?.Report(new LauncherProgress("업데이트 적용 완료", 100));
        return LauncherRunResult.Success("업데이트 완료");
    }

    public void LaunchMainApplication()
    {
        Process.Start(_options.CreateLaunchStartInfo());
    }

    private async Task<ReleaseManifest> DownloadRemoteManifestAsync(CancellationToken cancellationToken)
    {
        var manifestUrl = _options.ManifestUrl ?? throw new InvalidOperationException("ManifestUrl is not configured.");
        using var response = await SharedHttpClient.GetAsync(manifestUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var manifest = await JsonSerializer.DeserializeAsync<ReleaseManifest>(stream, JsonOptions, cancellationToken);

        if (manifest == null || manifest.Files.Count == 0)
        {
            throw new InvalidOperationException("릴리즈 manifest 형식이 올바르지 않습니다.");
        }

        manifest.Files = manifest.Files
            .Where(file => !_options.IsLauncherManagedFile(file.Path))
            .Select(file =>
            {
                file.Path = LauncherOptions.NormalizeRelativePath(file.Path);
                return file;
            })
            .ToList();

        return manifest;
    }

    private async Task<ManifestChangeSet> BuildChangeSetAsync(ReleaseManifest remoteManifest, CancellationToken cancellationToken)
    {
        var appRoot = _options.ResolveApplicationRoot();
        var currentManifest = await LoadInstalledManifestAsync(cancellationToken);
        var remoteMap = remoteManifest.Files.ToDictionary(file => file.Path, StringComparer.OrdinalIgnoreCase);

        var filesToDownload = new List<ReleaseFileEntry>();
        foreach (var entry in remoteManifest.Files)
        {
            var localPath = Path.Combine(appRoot, entry.Path.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(localPath))
            {
                filesToDownload.Add(entry);
                continue;
            }

            var localHash = await ComputeSha256Async(localPath, cancellationToken);
            if (!localHash.Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                filesToDownload.Add(entry);
            }
        }

        var filesToDelete = new List<string>();
        if (currentManifest != null)
        {
            foreach (var oldEntry in currentManifest.Files)
            {
                if (_options.IsLauncherManagedFile(oldEntry.Path))
                {
                    continue;
                }

                if (!remoteMap.ContainsKey(LauncherOptions.NormalizeRelativePath(oldEntry.Path)))
                {
                    filesToDelete.Add(LauncherOptions.NormalizeRelativePath(oldEntry.Path));
                }
            }
        }

        return new ManifestChangeSet(filesToDownload, filesToDelete);
    }

    private async Task ApplyChangesAsync(
        ReleaseManifest remoteManifest,
        ManifestChangeSet changeSet,
        IProgress<LauncherProgress>? progress,
        CancellationToken cancellationToken)
    {
        var appRoot = _options.ResolveApplicationRoot();
        var tempRoot = Path.Combine(Path.GetTempPath(), "Monthly_Excel_Launcher", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            for (var index = 0; index < changeSet.FilesToDownload.Count; index++)
            {
                var file = changeSet.FilesToDownload[index];
                var percent = GetPercent(index, changeSet.FilesToDownload.Count, 10, 75);
                progress?.Report(new LauncherProgress($"변경 파일 다운로드 중... ({index + 1}/{changeSet.FilesToDownload.Count})", percent));
                await DownloadFileToTempAsync(file, tempRoot, cancellationToken);
            }

            progress?.Report(new LauncherProgress("파일 교체 중...", 85));

            foreach (var file in changeSet.FilesToDownload)
            {
                var tempPath = Path.Combine(tempRoot, file.Path.Replace('/', Path.DirectorySeparatorChar));
                var destinationPath = Path.Combine(appRoot, file.Path.Replace('/', Path.DirectorySeparatorChar));
                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrWhiteSpace(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                File.Copy(tempPath, destinationPath, overwrite: true);
            }

            foreach (var relativePath in changeSet.FilesToDelete)
            {
                var localPath = Path.Combine(appRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }
            }

            await SaveInstalledManifestAsync(remoteManifest, cancellationToken);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private async Task DownloadFileToTempAsync(ReleaseFileEntry file, string tempRoot, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(file.DownloadUrl))
        {
            throw new InvalidOperationException($"다운로드 주소가 없는 파일이 있습니다: {file.Path}");
        }

        using var response = await SharedHttpClient.GetAsync(file.DownloadUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tempPath = Path.Combine(tempRoot, file.Path.Replace('/', Path.DirectorySeparatorChar));
        var tempDirectory = Path.GetDirectoryName(tempPath);
        if (!string.IsNullOrWhiteSpace(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        await using (var destination = File.Create(tempPath))
        {
            await response.Content.CopyToAsync(destination, cancellationToken);
        }

        var downloadedHash = await ComputeSha256Async(tempPath, cancellationToken);
        if (!downloadedHash.Equals(file.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"다운로드 검증에 실패했습니다: {file.Path}");
        }
    }

    private async Task<ReleaseManifest?> LoadInstalledManifestAsync(CancellationToken cancellationToken)
    {
        var manifestPath = _options.GetCurrentManifestPath();
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        await using var stream = File.OpenRead(manifestPath);
        return await JsonSerializer.DeserializeAsync<ReleaseManifest>(stream, JsonOptions, cancellationToken);
    }

    private async Task SaveInstalledManifestAsync(ReleaseManifest manifest, CancellationToken cancellationToken)
    {
        var manifestPath = _options.GetCurrentManifestPath();
        var directory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(manifestPath);
        await JsonSerializer.SerializeAsync(stream, manifest, JsonOptions, cancellationToken);
    }

    private static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken)
    {
        using var sha = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hash = await sha.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private static int GetPercent(int index, int total, int start, int end)
    {
        if (total <= 0)
        {
            return end;
        }

        var progress = (double)(index + 1) / total;
        return start + (int)Math.Round((end - start) * progress);
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("MonthlyExcelLauncher/1.0");
        return client;
    }

    private sealed record ManifestChangeSet(
        IReadOnlyList<ReleaseFileEntry> FilesToDownload,
        IReadOnlyList<string> FilesToDelete);
}
