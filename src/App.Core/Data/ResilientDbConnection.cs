using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DeskAppKit.Core.Data;

/// <summary>
/// 再試行機能を持つ回復性の高いデータベース接続ラッパー
/// </summary>
public class ResilientDbConnection : IDisposable
{
    private readonly IDbConnectionFactory _factory;
    private readonly DbConnectionSettings _settings;
    private IDbConnection? _connection;

    public ResilientDbConnection(IDbConnectionFactory factory, DbConnectionSettings settings)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// 再試行ロジック付きで接続を取得します
    /// </summary>
    public async Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection?.State == ConnectionState.Open)
        {
            return _connection;
        }

        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount < _settings.MaxRetryCount)
        {
            try
            {
                _connection = await _factory.CreateConnectionAsync(cancellationToken);

                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                return _connection;
            }
            catch (Exception ex) when (IsTransientError(ex))
            {
                lastException = ex;
                retryCount++;

                if (retryCount < _settings.MaxRetryCount)
                {
                    // 指数バックオフで待機
                    var delay = _settings.RetryDelayMs * (int)Math.Pow(2, retryCount - 1);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        throw new DbConnectionException(
            $"データベース接続に{_settings.MaxRetryCount}回失敗しました。",
            lastException);
    }

    /// <summary>
    /// 一時的なエラーかどうかを判定します
    /// </summary>
    private bool IsTransientError(Exception exception)
    {
        // 一般的な一時的エラーのパターン
        var message = exception.Message.ToLower();

        return message.Contains("timeout") ||
               message.Contains("network") ||
               message.Contains("connection was closed") ||
               message.Contains("transport-level error") ||
               message.Contains("deadlock") ||
               exception is TimeoutException;
    }

    /// <summary>
    /// トランザクション付きでコマンドを実行します
    /// </summary>
    public async Task<T> ExecuteWithTransactionAsync<T>(
        Func<IDbConnection, IDbTransaction, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            var result = await action(connection, transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// コマンドを実行します（再試行付き）
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<IDbConnection, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount < _settings.MaxRetryCount)
        {
            try
            {
                var connection = await GetConnectionAsync(cancellationToken);
                return await action(connection);
            }
            catch (Exception ex) when (IsTransientError(ex))
            {
                lastException = ex;
                retryCount++;

                // 接続を閉じて次の試行で再作成
                _connection?.Close();
                _connection?.Dispose();
                _connection = null;

                if (retryCount < _settings.MaxRetryCount)
                {
                    var delay = _settings.RetryDelayMs * (int)Math.Pow(2, retryCount - 1);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        throw new DbConnectionException(
            $"データベース操作に{_settings.MaxRetryCount}回失敗しました。",
            lastException);
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
    }
}

/// <summary>
/// データベース接続例外
/// </summary>
public class DbConnectionException : Exception
{
    public DbConnectionException(string message) : base(message)
    {
    }

    public DbConnectionException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
