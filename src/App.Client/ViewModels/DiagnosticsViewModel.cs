using System.Collections.ObjectModel;
using System.Windows.Input;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Diagnostics;

namespace DeskAppKit.Client.ViewModels;

/// <summary>
/// 診断画面ViewModel
/// </summary>
public class DiagnosticsViewModel : ViewModelBase
{
    private readonly ILogger _logger;
    private readonly HealthCheck? _healthCheck;
    private bool _isRunning;
    private string _statusMessage = string.Empty;

    public DiagnosticsViewModel(ILogger logger, HealthCheck? healthCheck = null)
    {
        _logger = logger;
        _healthCheck = healthCheck;

        HealthCheckResults = new ObservableCollection<HealthCheckResultViewModel>();
        RunHealthCheckCommand = new RelayCommand(async () => await RunHealthCheckAsync(), () => !IsRunning);

        // 初期メッセージ設定
        if (_healthCheck == null)
        {
            StatusMessage = "Localモード: ログ機能は利用可能です。データベースヘルスチェックは利用できません。";
        }
        else
        {
            StatusMessage = "診断を開始するには「ヘルスチェック実行」ボタンをクリックしてください";
        }
    }

    /// <summary>
    /// ヘルスチェック結果リスト
    /// </summary>
    public ObservableCollection<HealthCheckResultViewModel> HealthCheckResults { get; }

    /// <summary>
    /// 実行中フラグ
    /// </summary>
    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }

    /// <summary>
    /// ステータスメッセージ
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// ヘルスチェック実行コマンド
    /// </summary>
    public ICommand RunHealthCheckCommand { get; }

    /// <summary>
    /// ヘルスチェック実行
    /// </summary>
    private async Task RunHealthCheckAsync()
    {
        if (_healthCheck == null)
        {
            StatusMessage = "データベースヘルスチェック機能が利用できません（Localモード）。ログ機能は正常に動作しています。";
            _logger.Info("DiagnosticsViewModel", "Localモードのため、ヘルスチェックはスキップされました");
            return;
        }

        try
        {
            IsRunning = true;
            StatusMessage = "ヘルスチェックを実行中...";
            HealthCheckResults.Clear();

            var results = await _healthCheck.RunAllChecksAsync();

            foreach (var result in results)
            {
                HealthCheckResults.Add(new HealthCheckResultViewModel(result));
            }

            var healthyCount = results.Count(r => r.IsHealthy);
            var totalCount = results.Count;

            StatusMessage = $"ヘルスチェック完了: {healthyCount}/{totalCount} 正常";
            _logger.Info("DiagnosticsViewModel", $"ヘルスチェック実行完了: {healthyCount}/{totalCount} 正常");
        }
        catch (Exception ex)
        {
            StatusMessage = $"エラー: {ex.Message}";
            _logger.Error("DiagnosticsViewModel", "ヘルスチェック実行中にエラーが発生しました", ex);
        }
        finally
        {
            IsRunning = false;
        }
    }
}

/// <summary>
/// ヘルスチェック結果表示用ViewModel
/// </summary>
public class HealthCheckResultViewModel
{
    public HealthCheckResultViewModel(HealthCheckResult result)
    {
        Component = result.Component;
        IsHealthy = result.IsHealthy;
        Message = result.Message;
        ResponseTime = result.ResponseTime.TotalMilliseconds;
        StatusText = IsHealthy ? "✓ 正常" : "✗ 異常";
        StatusColor = IsHealthy ? "#4CAF50" : "#F44336";
    }

    public string Component { get; }
    public bool IsHealthy { get; }
    public string Message { get; }
    public double ResponseTime { get; }
    public string StatusText { get; }
    public string StatusColor { get; }
}
