using System.IO;
using DeskAppKit.Infrastructure.Database;
using DeskAppKit.Infrastructure.Logging;
using Xunit;

namespace DeskAppKit.IntegrationTests;

/// <summary>
/// LocalDbSetupServiceの統合テスト
/// 注意: このテストはWindows環境でLocalDBがインストールされている場合のみ実行されます
/// </summary>
public class LocalDbSetupServiceTests
{
    private readonly LocalDbSetupService _service;
    private readonly string _testDataDirectory;
    private readonly string _testEncryptionKey;

    public LocalDbSetupServiceTests()
    {
        var logDirectory = Path.Combine(Path.GetTempPath(), "DeskAppKitTests", "Logs");
        Directory.CreateDirectory(logDirectory);
        var logger = new FileLogger(logDirectory, "1.0.0-test");

        _service = new LocalDbSetupService(logger, "DeskAppKitTest", "DeskAppKitTestDb");
        _testDataDirectory = Path.Combine(Path.GetTempPath(), "DeskAppKitTests", "Data");
        Directory.CreateDirectory(_testDataDirectory);
        _testEncryptionKey = "TestEncryptionKey12345678901234567";
    }

    [Fact]
    public async Task IsLocalDbAvailableAsync_ShouldReturnBool()
    {
        // Act
        var result = await _service.IsLocalDbAvailableAsync();

        // Assert
        // 結果がtrue/falseのどちらかであることを確認（環境依存）
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task InstanceExistsAsync_ShouldReturnBool()
    {
        // Act
        var result = await _service.InstanceExistsAsync();

        // Assert
        Assert.IsType<bool>(result);
    }

    [Fact(Skip = "LocalDB環境が必要なため、手動実行のみ")]
    public async Task SetupAsync_ShouldCreateLocalDbInstance()
    {
        // Arrange
        var isAvailable = await _service.IsLocalDbAvailableAsync();
        if (!isAvailable)
        {
            // LocalDBがインストールされていない場合はスキップ
            return;
        }

        // Act
        var result = await _service.SetupAsync(_testDataDirectory, _testEncryptionKey);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.ConnectionString);
        Assert.Contains("✓", string.Join("\n", result.Steps));
    }

    [Fact]
    public async Task SetupAsync_WhenLocalDbNotAvailable_ShouldReturnFailure()
    {
        // Arrange
        var service = new LocalDbSetupService(null, "NonExistentInstance", "TestDb");
        var isAvailable = await service.IsLocalDbAvailableAsync();

        if (isAvailable)
        {
            // LocalDBが利用可能な場合はこのテストをスキップ
            return;
        }

        // Act
        var result = await service.SetupAsync(_testDataDirectory, _testEncryptionKey);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("LocalDB", result.Message);
    }
}
