using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using DeskAppKit.Core.Interfaces;

namespace DeskAppKit.Client.ViewModels;

/// <summary>
/// データ一覧ViewModel
/// </summary>
public class DataListViewModel : ViewModelBase
{
    private readonly IDataListService _dataListService;
    private readonly ILogger? _logger;
    private string _sqlQuery = string.Empty;
    private DataTable? _resultData;
    private int _rowCount;
    private bool _isLoading;
    private string _statusMessage = string.Empty;

    public DataListViewModel(IDataListService dataListService, ILogger? logger = null)
    {
        _dataListService = dataListService;
        _logger = logger;

        // コマンド初期化
        ExecuteQueryCommand = new RelayCommand(async () => await ExecuteQueryAsync(), () => !IsLoading && !string.IsNullOrWhiteSpace(SqlQuery));
        ClearCommand = new RelayCommand(Clear, () => !IsLoading);

        // サンプルクエリを設定
        SqlQuery = "SELECT TOP 100 * FROM Users";
    }

    /// <summary>
    /// SQLクエリ
    /// </summary>
    public string SqlQuery
    {
        get => _sqlQuery;
        set
        {
            if (SetProperty(ref _sqlQuery, value))
            {
                ((RelayCommand)ExecuteQueryCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 結果データ
    /// </summary>
    public DataTable? ResultData
    {
        get => _resultData;
        set => SetProperty(ref _resultData, value);
    }

    /// <summary>
    /// 行数
    /// </summary>
    public int RowCount
    {
        get => _rowCount;
        set => SetProperty(ref _rowCount, value);
    }

    /// <summary>
    /// ローディング中フラグ
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                ((RelayCommand)ExecuteQueryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ClearCommand).RaiseCanExecuteChanged();
            }
        }
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
    /// クエリ実行コマンド
    /// </summary>
    public ICommand ExecuteQueryCommand { get; }

    /// <summary>
    /// クリアコマンド
    /// </summary>
    public ICommand ClearCommand { get; }

    /// <summary>
    /// クエリを実行
    /// </summary>
    private async Task ExecuteQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(SqlQuery))
        {
            MessageBox.Show("SQLクエリを入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "クエリ実行中...";

            var result = await _dataListService.ExecuteQueryAsync(SqlQuery);

            ResultData = result;
            RowCount = result.Rows.Count;
            StatusMessage = $"クエリ実行完了: {RowCount}件取得";

            _logger?.Info("DataListViewModel", $"クエリ実行成功: {RowCount}件");
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "クエリエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"エラー: {ex.Message}";
            _logger?.Error("DataListViewModel", "クエリ実行エラー", ex);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"予期しないエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"エラー: {ex.Message}";
            _logger?.Error("DataListViewModel", "予期しないエラー", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// クリア
    /// </summary>
    private void Clear()
    {
        ResultData = null;
        RowCount = 0;
        StatusMessage = string.Empty;
    }
}
