using System.Text.Json.Serialization;

namespace Monthly_Excel.Launcher;

internal sealed class ReleaseManifest
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<ReleaseFileEntry> Files { get; set; } = new();
}

internal sealed class ReleaseFileEntry
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }
}
