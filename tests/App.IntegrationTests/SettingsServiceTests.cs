using DeskAppKit.Core.Enums;
using DeskAppKit.Infrastructure.Settings;
using System.IO;

namespace App.IntegrationTests;

/// <summary>
/// 設定サービスの統合テスト
/// </summary>
public class SettingsServiceTests : IDisposable
{
    private readonly string _testDataDirectory;
    private readonly string _encryptionKey = "TestEncryptionKey123456789012345678901234567890"; // 32文字以上

    public SettingsServiceTests()
    {
        _testDataDirectory = Path.Combine(Path.GetTempPath(), $"DeskAppKitTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataDirectory);
    }

    [Fact]
    public void LocalMode_設定の読み書き()
    {
        // Arrange
        var settingsService = new SettingsService(_testDataDirectory, _encryptionKey);

        // Act - 設定を書き込み
        settingsService.Set("App", "TestKey1", "TestValue1");
        settingsService.Set("App", "TestKey2", 12345);
        settingsService.Set("User", "TestKey3", true);
        settingsService.Save();

        // 新しいインスタンスで読み込み
        var settingsService2 = new SettingsService(_testDataDirectory, _encryptionKey);

        // Assert
        Assert.Equal("TestValue1", settingsService2.Get<string>("App", "TestKey1"));
        Assert.Equal(12345, settingsService2.Get<int>("App", "TestKey2"));
        Assert.True(settingsService2.Get<bool>("User", "TestKey3"));
    }

    [Fact]
    public void LocalMode_デフォルト値の取得()
    {
        // Arrange
        var settingsService = new SettingsService(_testDataDirectory, _encryptionKey);

        // Act & Assert
        Assert.Equal("DefaultValue", settingsService.Get("App", "NonExistentKey", "DefaultValue"));
        Assert.Equal(999, settingsService.Get("App", "NonExistentKey", 999));
    }

    [Fact]
    public void LocalMode_ユーザー別設定の読み書き()
    {
        // Arrange
        var settingsService = new SettingsService(_testDataDirectory, _encryptionKey);
        var userId1 = Guid.NewGuid();

        // Act - LocalModeではUserIdに関わらず同じユーザー設定領域を共有
        settingsService.SetUser(userId1, "UI", "Theme", "Dark");
        settingsService.Save();

        // 新しいインスタンスで読み込み
        var settingsService2 = new SettingsService(_testDataDirectory, _encryptionKey);

        // Assert - LocalModeではUserIdパラメータは使われず、同じ設定が読み込まれる
        Assert.Equal("Dark", settingsService2.GetUser<string>(userId1, "UI", "Theme"));

        // 別のUserIdでも同じ値が返される（LocalModeの制限）
        var userId2 = Guid.NewGuid();
        Assert.Equal("Dark", settingsService2.GetUser<string>(userId2, "UI", "Theme"));
    }

    [Fact]
    public void GetStorageMode_デフォルトはLocal()
    {
        // Arrange & Act
        var settingsService = new SettingsService(_testDataDirectory, _encryptionKey);

        // Assert
        Assert.Equal(StorageMode.Local, settingsService.GetStorageMode());
    }

    [Fact]
    public void 設定の上書き()
    {
        // Arrange
        var settingsService = new SettingsService(_testDataDirectory, _encryptionKey);

        // Act
        settingsService.Set("App", "OverwriteKey", "Value1");
        settingsService.Save();

        Assert.Equal("Value1", settingsService.Get<string>("App", "OverwriteKey"));

        settingsService.Set("App", "OverwriteKey", "Value2");
        settingsService.Save();

        // Assert
        Assert.Equal("Value2", settingsService.Get<string>("App", "OverwriteKey"));
    }

    [Fact]
    public void 複数カテゴリの設定管理()
    {
        // Arrange
        var settingsService = new SettingsService(_testDataDirectory, _encryptionKey);

        // Act
        settingsService.Set("App", "Key1", "AppValue");
        settingsService.Set("UI", "Key1", "UIValue");
        settingsService.Set("Security", "Key1", "SecurityValue");
        settingsService.Save();

        // Assert
        Assert.Equal("AppValue", settingsService.Get<string>("App", "Key1"));
        Assert.Equal("UIValue", settingsService.Get<string>("UI", "Key1"));
        Assert.Equal("SecurityValue", settingsService.Get<string>("Security", "Key1"));
    }

    [Fact]
    public void 型変換テスト()
    {
        // Arrange
        var settingsService = new SettingsService(_testDataDirectory, _encryptionKey);

        // Act & Assert - 文字列
        settingsService.Set("Test", "StringKey", "StringValue");
        settingsService.Save();
        Assert.Equal("StringValue", settingsService.Get<string>("Test", "StringKey"));

        // Act & Assert - 整数
        settingsService.Set("Test", "IntKey", 42);
        settingsService.Save();
        Assert.Equal(42, settingsService.Get<int>("Test", "IntKey"));

        // Act & Assert - bool
        settingsService.Set("Test", "BoolKey", true);
        settingsService.Save();
        Assert.True(settingsService.Get<bool>("Test", "BoolKey"));

        // Act & Assert - double
        settingsService.Set("Test", "DoubleKey", 3.14159);
        settingsService.Save();
        Assert.Equal(3.14159, settingsService.Get<double>("Test", "DoubleKey"), 5);
    }

    [Fact]
    public void Load後の設定取得()
    {
        // Arrange
        var settingsService1 = new SettingsService(_testDataDirectory, _encryptionKey);
        settingsService1.Set("App", "Key", "Value");
        settingsService1.Save();

        // Act
        var settingsService2 = new SettingsService(_testDataDirectory, _encryptionKey);
        settingsService2.Load(); // 明示的にロード

        // Assert
        Assert.Equal("Value", settingsService2.Get<string>("App", "Key"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDirectory))
        {
            try
            {
                Directory.Delete(_testDataDirectory, true);
            }
            catch
            {
                // テスト後のクリーンアップエラーは無視
            }
        }
    }
}
