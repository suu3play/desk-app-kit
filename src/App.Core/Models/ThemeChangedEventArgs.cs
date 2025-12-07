using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Models;

/// <summary>
/// テーマ変更イベント引数
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    /// <summary>
    /// 変更前のテーマモード
    /// </summary>
    public ThemeMode OldMode { get; }

    /// <summary>
    /// 変更後のテーマモード
    /// </summary>
    public ThemeMode NewMode { get; }

    /// <summary>
    /// 変更後のテーマ
    /// </summary>
    public Theme NewTheme { get; }

    public ThemeChangedEventArgs(ThemeMode oldMode, ThemeMode newMode, Theme newTheme)
    {
        OldMode = oldMode;
        NewMode = newMode;
        NewTheme = newTheme ?? throw new ArgumentNullException(nameof(newTheme));
    }
}
