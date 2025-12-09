using System.Data;
using System.Data.Common;
using DeskAppKit.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DeskAppKit.Infrastructure.Data;

/// <summary>
/// データ一覧表示サービス実装
/// </summary>
public class DataListService : IDataListService
{
    private readonly string _connectionString;
    private readonly ILogger? _logger;

    public DataListService(IConfiguration configuration, ILogger? logger = null)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    /// <summary>
    /// SQLクエリを実行してDataTableを取得
    /// </summary>
    public async Task<DataTable> ExecuteQueryAsync(string sql, Dictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQLクエリが空です。", nameof(sql));
        }

        // SELECTクエリのみを許可（セキュリティ対策）
        var trimmedSql = sql.Trim().ToUpperInvariant();
        if (!trimmedSql.StartsWith("SELECT"))
        {
            throw new InvalidOperationException("SELECT文のみ実行可能です。");
        }

        var dataTable = new DataTable();

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 30;

            // パラメータを設定
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            await using var reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);

            _logger?.Info("DataListService", $"クエリ実行成功: {dataTable.Rows.Count}件取得");

            return dataTable;
        }
        catch (SqlException ex)
        {
            _logger?.Error("DataListService", $"SQLエラー: {ex.Message}", ex);
            throw new InvalidOperationException($"データベースエラーが発生しました: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger?.Error("DataListService", $"クエリ実行エラー: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// テーブルの行数を取得
    /// </summary>
    public async Task<int> GetRowCountAsync(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("テーブル名が空です。", nameof(tableName));
        }

        // SQLインジェクション対策: テーブル名の検証
        if (!IsValidTableName(tableName))
        {
            throw new ArgumentException("無効なテーブル名です。", nameof(tableName));
        }

        try
        {
            // テーブル名は検証済みなので直接埋め込み（パラメータ化できないため）
            var sql = $"SELECT COUNT(*) FROM {tableName}";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 30;

            var result = await command.ExecuteScalarAsync();
            var count = Convert.ToInt32(result);

            _logger?.Info("DataListService", $"行数取得成功: {tableName} = {count}件");

            return count;
        }
        catch (SqlException ex)
        {
            _logger?.Error("DataListService", $"SQLエラー: {ex.Message}", ex);
            throw new InvalidOperationException($"データベースエラーが発生しました: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger?.Error("DataListService", $"行数取得エラー: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// テーブル名の妥当性を検証
    /// </summary>
    private static bool IsValidTableName(string tableName)
    {
        // 英数字、アンダースコア、ドット（スキーマ区切り）のみ許可
        return System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[a-zA-Z0-9_\.]+$");
    }
}
