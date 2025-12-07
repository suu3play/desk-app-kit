using System.Windows;
using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;
using Microsoft.Win32;

namespace DeskAppKit.Infrastructure.Themes;

/// <summary>
/// テーマサービス実装
/// </summary>
public class ThemeService : IThemeService
{
    private readonly Dictionary<ThemeMode, Theme> _themes;
    private readonly ISettingsService _settingsService;
    private ThemeMode _currentMode;
    private Theme _currentTheme;

    public Theme CurrentTheme => _currentTheme;
    public ThemeMode CurrentMode => _currentMode;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public ThemeService(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _themes = new Dictionary<ThemeMode, Theme>();

        InitializeDefaultThemes();

        _currentMode = ThemeMode.Light;
        _currentTheme = _themes[ThemeMode.Light];
    }

    /// <summary>
    /// デフォルトテーマの初期化
    /// </summary>
    private void InitializeDefaultThemes()
    {
        var lightTheme = new Theme
        {
            Name = "Light",
            Mode = ThemeMode.Light,
            Colors = new Dictionary<string, string>
            {
                { "Background", "#FFFFFF" },
                { "Foreground", "#000000" },
                { "Primary", "#2196F3" },
                { "PrimaryDark", "#1976D2" },
                { "Secondary", "#757575" },
                { "Border", "#E0E0E0" },
                { "CardBackground", "#F5F5F5" },
                { "Hover", "#E3F2FD" }
            },
            ResourcePath = "pack://application:,,,/App.Infrastructure;component/Themes/LightTheme.xaml",
            Description = "明るい配色のテーマ"
        };

        var darkTheme = new Theme
        {
            Name = "Dark",
            Mode = ThemeMode.Dark,
            Colors = new Dictionary<string, string>
            {
                { "Background", "#1E1E1E" },
                { "Foreground", "#FFFFFF" },
                { "Primary", "#64B5F6" },
                { "PrimaryDark", "#42A5F5" },
                { "Secondary", "#BDBDBD" },
                { "Border", "#424242" },
                { "CardBackground", "#2D2D2D" },
                { "Hover", "#424242" }
            },
            ResourcePath = "pack://application:,,,/App.Infrastructure;component/Themes/DarkTheme.xaml",
            Description = "暗い配色のテーマ"
        };

        _themes[ThemeMode.Light] = lightTheme;
        _themes[ThemeMode.Dark] = darkTheme;
    }

    public void ApplyTheme(ThemeMode mode)
    {
        var oldMode = _currentMode;

        var targetMode = mode == ThemeMode.System ? GetSystemTheme() : mode;

        if (!_themes.TryGetValue(targetMode, out var theme))
        {
            throw new ArgumentException($"テーマ '{targetMode}' が見つかりません。", nameof(mode));
        }

        _currentMode = mode;
        _currentTheme = theme;

        ApplyThemeToApplication(theme);

        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(oldMode, mode, theme));
    }

    public void RegisterTheme(Theme theme)
    {
        if (theme == null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        _themes[theme.Mode] = theme;
    }

    public ThemeMode GetSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var value = key.GetValue("AppsUseLightTheme");
                if (value is int appsUseLightTheme)
                {
                    return appsUseLightTheme == 1 ? ThemeMode.Light : ThemeMode.Dark;
                }
            }
        }
        catch
        {
            // レジストリアクセス失敗時はデフォルトでライトモード
        }

        return ThemeMode.Light;
    }

    public void LoadSavedTheme()
    {
        try
        {
            var savedMode = _settingsService.Get("UI", "Theme", "Light");
            if (Enum.TryParse<ThemeMode>(savedMode, out var mode))
            {
                ApplyTheme(mode);
            }
        }
        catch
        {
            ApplyTheme(ThemeMode.Light);
        }
    }

    public void SaveCurrentTheme()
    {
        try
        {
            _settingsService.Set("UI", "Theme", _currentMode.ToString());
            _settingsService.Save();
        }
        catch
        {
            // 保存失敗は無視
        }
    }

    /// <summary>
    /// アプリケーションにテーマを適用
    /// </summary>
    private void ApplyThemeToApplication(Theme theme)
    {
        if (Application.Current == null)
        {
            return;
        }

        var dictionaries = Application.Current.Resources.MergedDictionaries;

        var existingTheme = dictionaries.FirstOrDefault(d =>
            d.Source?.OriginalString.Contains("Theme.xaml") == true);

        if (existingTheme != null)
        {
            dictionaries.Remove(existingTheme);
        }

        if (!string.IsNullOrEmpty(theme.ResourcePath))
        {
            try
            {
                var themeDict = new ResourceDictionary
                {
                    Source = new Uri(theme.ResourcePath, UriKind.Absolute)
                };
                dictionaries.Add(themeDict);
            }
            catch
            {
                // リソース読み込み失敗時は無視
            }
        }
    }
}
