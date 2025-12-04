using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DeskAppKit.Core.Data;

/// <summary>
/// データベース接続ファクトリーインターフェース
/// DB種類に依存しない抽象化層を提供します
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// 新しいデータベース接続を作成します
    /// </summary>
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 接続文字列を取得します
    /// </summary>
    string GetConnectionString();
}

/// <summary>
/// データベース接続設定
/// </summary>
public class DbConnectionSettings
{
    /// <summary>
    /// 接続文字列
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 接続タイムアウト（秒）
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// コマンドタイムアウト（秒）
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// 最大再試行回数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 再試行間隔（ミリ秒）
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// 接続プーリングを有効化
    /// </summary>
    public bool EnablePooling { get; set; } = true;

    /// <summary>
    /// 最小プールサイズ
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// 最大プールサイズ
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;
}
