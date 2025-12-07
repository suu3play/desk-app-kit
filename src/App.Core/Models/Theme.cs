using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Models;

/// <summary>
/// テーマモデル
/// </summary>
public class Theme
{
    /// <summary>
    /// テーマ名
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// テーマモード
    /// </summary>
    public required ThemeMode Mode { get; set; }

    /// <summary>
    /// テーマカラーの定義
    /// </summary>
    public required Dictionary<string, string> Colors { get; set; }

    /// <summary>
    /// リソースディクショナリのパス
    /// </summary>
    public string? ResourcePath { get; set; }

    /// <summary>
    /// テーマの説明
    /// </summary>
    public string? Description { get; set; }
}
