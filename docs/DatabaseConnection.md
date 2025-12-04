# データベース接続機能

## 概要

DeskAppKitは、DB種類に依存しない抽象化層と、自動再試行機能を備えた回復性の高いデータベース接続機能を提供します。

## 主な機能

### 1. DB種類に依存しない抽象化

`IDbConnectionFactory`インターフェースにより、SQL Server、PostgreSQL、MySQL、SQLiteなど、任意のデータベースに対応できます。

### 2. 自動再試行機能

一時的なネットワークエラーやタイムアウトが発生した場合、自動的に再試行します。

### 3. 指数バックオフ

再試行の間隔を徐々に延ばすことで、サーバーへの負荷を軽減します。

### 4. 接続プーリング

効率的なリソース管理のため、接続プーリングをサポートします。

## 設定

### DbConnectionSettings

```csharp
using DeskAppKit.Core.Data;

var settings = new DbConnectionSettings
{
    ConnectionString = "Server=localhost;Database=mydb;User Id=user;Password=pass;",
    ConnectionTimeout = 30,      // 接続タイムアウト（秒）
    CommandTimeout = 30,         // コマンドタイムアウト（秒）
    MaxRetryCount = 3,           // 最大再試行回数
    RetryDelayMs = 1000,         // 初回再試行間隔（ミリ秒）
    EnablePooling = true,        // 接続プーリングを有効化
    MinPoolSize = 0,             // 最小プールサイズ
    MaxPoolSize = 100            // 最大プールサイズ
};
```

## 使用例

### 基本的な使用方法

```csharp
using DeskAppKit.Core.Data;
using System.Data.SqlClient;

// SQL Server用ファクトリーの実装例
public class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly DbConnectionSettings _settings;

    public SqlServerConnectionFactory(DbConnectionSettings settings)
    {
        _settings = settings;
    }

    public Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(GetConnectionString());
        return Task.FromResult<IDbConnection>(connection);
    }

    public string GetConnectionString()
    {
        return _settings.ConnectionString;
    }
}

// 使用
var factory = new SqlServerConnectionFactory(settings);
using var resilientConnection = new ResilientDbConnection(factory, settings);

// 接続取得（自動再試行付き）
var connection = await resilientConnection.GetConnectionAsync();
```

### クエリ実行（再試行付き）

```csharp
var result = await resilientConnection.ExecuteAsync(async conn =>
{
    using var command = conn.CreateCommand();
    command.CommandText = "SELECT * FROM Users WHERE Id = @Id";
    command.Parameters.Add(new SqlParameter("@Id", userId));

    using var reader = await command.ExecuteReaderAsync();
    // データ読み取り処理
    return ProcessData(reader);
});
```

### トランザクション処理

```csharp
var result = await resilientConnection.ExecuteWithTransactionAsync(
    async (conn, transaction) =>
    {
        using var command = conn.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "UPDATE Users SET Name = @Name WHERE Id = @Id";
        command.Parameters.Add(new SqlParameter("@Name", newName));
        command.Parameters.Add(new SqlParameter("@Id", userId));

        await command.ExecuteNonQueryAsync();

        return true;
    });
```

## 対応データベース例

### SQL Server

```csharp
using System.Data.SqlClient;

public class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IDbConnection>(new SqlConnection(_connectionString));
    }

    public string GetConnectionString() => _connectionString;
}
```

### PostgreSQL

```csharp
using Npgsql;

public class PostgreSqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgreSqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IDbConnection>(new NpgsqlConnection(_connectionString));
    }

    public string GetConnectionString() => _connectionString;
}
```

### SQLite

```csharp
using System.Data.SQLite;

public class SQLiteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SQLiteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IDbConnection>(new SQLiteConnection(_connectionString));
    }

    public string GetConnectionString() => _connectionString;
}
```

## 一時的エラーの判定

以下のエラーは一時的なものとして自動的に再試行されます：

- タイムアウトエラー
- ネットワークエラー
- 接続クローズエラー
- トランスポート層エラー
- デッドロック

## エラーハンドリング

再試行回数が上限に達すると、`DbConnectionException`がスローされます。

```csharp
try
{
    var connection = await resilientConnection.GetConnectionAsync();
}
catch (DbConnectionException ex)
{
    // 接続失敗時の処理
    Console.WriteLine($"データベース接続に失敗しました: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"詳細: {ex.InnerException.Message}");
    }
}
```

## ベストプラクティス

1. **適切なタイムアウト設定**: 環境に応じて適切な値を設定する
2. **再試行回数の調整**: ネットワーク状況に応じて調整する
3. **接続プーリングの活用**: パフォーマンス向上のため有効化する
4. **トランザクションの適切な使用**: データ整合性が重要な操作で使用する
5. **リソースの適切な解放**: using文やDisposeを使用して確実にリソースを解放する

## DapperやEntity Framework Coreとの統合

### Dapper

```csharp
using Dapper;

var users = await resilientConnection.ExecuteAsync(async conn =>
{
    return await conn.QueryAsync<User>("SELECT * FROM Users WHERE Active = @Active",
        new { Active = true });
});
```

### Entity Framework Core

```csharp
// DbContextのOnConfiguringメソッドで使用
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    var factory = new SqlServerConnectionFactory(settings);
    var resilientConnection = new ResilientDbConnection(factory, settings);
    var connection = resilientConnection.GetConnectionAsync().Result;

    optionsBuilder.UseSqlServer((SqlConnection)connection);
}
```

## パフォーマンス最適化

### 接続プーリング設定

```csharp
var settings = new DbConnectionSettings
{
    ConnectionString = "...",
    EnablePooling = true,
    MinPoolSize = 5,      // 常時5接続を維持
    MaxPoolSize = 50      // 最大50接続まで拡張
};
```

### コマンドタイムアウトの調整

```csharp
var settings = new DbConnectionSettings
{
    ConnectionString = "...",
    CommandTimeout = 60   // 長時間かかるクエリ用に60秒に設定
};
```
