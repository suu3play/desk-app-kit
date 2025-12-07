using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// テーマサービスインターフェース
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// 現在のテーマ
    /// </summary>
    Theme CurrentTheme { get; }

    /// <summary>
    /// 現在のテーマモード
    /// </summary>
    ThemeMode CurrentMode { get; }

    /// <summary>
    /// テーマ変更イベント
    /// </summary>
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// テーマを適用
    /// </summary>
    /// <param name="mode">テーマモード</param>
    void ApplyTheme(ThemeMode mode);

    /// <summary>
    /// テーマを登録
    /// </summary>
    /// <param name="theme">テーマ</param>
    void RegisterTheme(Theme theme);

    /// <summary>
    /// システムテーマを取得
    /// </summary>
    /// <returns>システムのテーマモード</returns>
    ThemeMode GetSystemTheme();

    /// <summary>
    /// 保存されたテーマ設定を読み込み
    /// </summary>
    void LoadSavedTheme();

    /// <summary>
    /// 現在のテーマ設定を保存
    /// </summary>
    void SaveCurrentTheme();
}
