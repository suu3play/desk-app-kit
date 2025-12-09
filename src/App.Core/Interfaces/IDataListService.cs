using System.Data;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// データ一覧表示サービスインターフェース
/// </summary>
public interface IDataListService
{
    /// <summary>
    /// SQLクエリを実行してDataTableを取得
    /// </summary>
    Task<DataTable> ExecuteQueryAsync(string sql, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// テーブルの行数を取得
    /// </summary>
    Task<int> GetRowCountAsync(string tableName);
}
