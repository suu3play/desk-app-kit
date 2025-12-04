namespace DeskAppKit.Infrastructure.Update;

/// <summary>
/// バージョン情報
/// </summary>
public class VersionInfo
{
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// バージョン文字列を比較
    /// </summary>
    public static int CompareVersions(string version1, string version2)
    {
        var v1Parts = version1.Split('.');
        var v2Parts = version2.Split('.');

        var maxLength = Math.Max(v1Parts.Length, v2Parts.Length);

        for (int i = 0; i < maxLength; i++)
        {
            var v1Part = i < v1Parts.Length ? int.Parse(v1Parts[i]) : 0;
            var v2Part = i < v2Parts.Length ? int.Parse(v2Parts[i]) : 0;

            if (v1Part > v2Part)
                return 1;
            if (v1Part < v2Part)
                return -1;
        }

        return 0;
    }

    /// <summary>
    /// より新しいバージョンかチェック
    /// </summary>
    public bool IsNewerThan(string currentVersion)
    {
        return CompareVersions(Version, currentVersion) > 0;
    }
}
