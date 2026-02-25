# DeskAppKit - デスクトップアプリケーション共通基盤

C#（WPF）デスクトップアプリケーションの再利用可能な共通基盤フレームワーク

## 概要

小規模〜中規模のデスクトップアプリケーション開発において、以下の機能を標準化・共通化します:

- ログ・設定管理
- DB接続・認証・セキュリティ
- 自動アップデート/ダウングレード
- Local/Databaseの2モード対応（フェイルセーフ機能付き）
- ヘルスチェック・診断機能

## プロジェクト構成

```
DeskAppKit/
├── src/
│   ├── App.Core/              # ドメインモデル・インターフェイス層
│   │   ├── Enums/            # 列挙型
│   │   ├── Models/           # エンティティモデル
│   │   └── Interfaces/       # サービスインターフェイス
│   ├── App.Infrastructure/    # 実装層
│   │   ├── Persistence/      # EF Core, リポジトリ
│   │   ├── Logging/          # ファイル/DBロガー
│   │   ├── Settings/         # Local/Database設定管理
│   │   ├── Security/         # 認証・暗号化
│   │   ├── Update/           # 自動アップデート
│   │   └── Diagnostics/      # ヘルスチェック
│   └── App.Client/           # WPF UI層
│       ├── Views/            # XAML画面
│       ├── ViewModels/       # ViewModel
│       └── Services/         # UIサービス
├── docs/
│   └── 仕様書.md             # 詳細仕様
└── DeskAppKit.sln
```

## 技術スタック

- **.NET 8.0**
- **WPF** (MVVM)
- **Entity Framework Core 8.0** (SQL Server)
- **BCrypt.Net** (パスワードハッシュ)
- **AES-256** (設定暗号化)

## 主要機能

### 1. StorageMode（Local / Database）

アプリ全体の設定・ログ保存方式を切り替え可能:

- **Local**: JSON設定ファイル + ファイルログ
- **Database**: Settingsテーブル + Logsテーブル

**フェイルセーフ**: Database接続失敗時は自動的にLocalモードにフォールバック

### 2. 設定管理

- `settings_app.json` - アプリケーション設定
- `settings_user.json` - ユーザー別設定
- `bootstrap_db.json` - DB接続情報（暗号化）
- すべての値はAES-256で暗号化

### 3. ログ管理

- JSON Lines形式のローカルログ
- ログレベル: Debug / Info / Warn / Error
- ログ項目: Timestamp, Level, Logger, Message, Exception, UserId, MachineName, AppVersion, ContextJson
- 自動ローテーション・古いログ削除

### 4. 認証・セキュリティ

- BCryptによるパスワードハッシュ化（Salt付き）
- アカウント状態管理: Active / Locked / Suspended / Retired
- ログイン失敗5回でアカウントロック（30分）
- 権限ロール: Admin / User / Viewer

### 5. データベース

**テーブル構成**:
- `Users` - ユーザー情報
- `Settings` - 設定情報
- `Logs` - ログ情報
- `SchemaVersion` - スキーマバージョン管理
- `ClientVersionStatus` - クライアントバージョン状態

## 実装済み機能（第1〜7段階）

### ✅ Core層
- すべてのEnums（StorageMode, AccountStatus, UserRole, LogLevel等）
- すべてのModels（User, Setting, Log, SchemaVersion, ClientVersionStatus）
- 主要インターフェイス（ILogger, ISettingsService, IAuthenticationService, IRepository）

### ✅ Infrastructure層
- **暗号化**: AES-256暗号化ヘルパー
- **設定管理**:
  - LocalSettingsStore（JSON）
  - BootstrapDbManager（DB接続情報）
  - SettingsService（StorageMode切り替え + フェイルセーフ）
- **ログ管理**: FileLogger（JSON Lines）
- **DB**: AppDbContext, 汎用Repository
- **セキュリティ**: PasswordHasher, AuthenticationService
- **自動アップデート**:
  - VersionInfo, UpdateChecker（最新バージョンチェック）
  - PackageDownloader（ダウンロード + SHA256検証）
  - InstallerRunner（インストーラー実行）
  - UpdateService（統合サービス）
- **診断機能**:
  - HealthCheck（DB接続、アップデートサーバー、ディスク、メモリ）
  - EnvironmentInfo（環境情報収集、エラーレポートZIP生成）

### ✅ エラーハンドリング
- **グローバル例外ハンドラー**: アプリケーション全体の未処理例外を統一的に処理
- **例外通知システム**: エラー発生時の自動通知機能
- **ファイルログ記録**: すべての例外を自動的にファイルに記録
- **ユーザーフレンドリーメッセージ**: 例外の種類に応じた適切なメッセージ生成

### ✅ データベース接続
- **DB種類非依存**: SQL Server、PostgreSQL、MySQL、SQLiteなど任意のDBに対応
- **自動再試行機能**: 一時的なエラーに対する自動リトライ（指数バックオフ）
- **接続プーリング**: 効率的なリソース管理
- **トランザクションサポート**: データ整合性を保証する安全な操作

## 未実装機能（今後の開発）

- WPF UI（ログイン画面の完全実装）
- DBマイグレーション自動実行
- 統合テスト

## ビルド

```bash
dotnet build DeskAppKit.sln
```

## 使用例

### 設定サービス

```csharp
var settingsService = new SettingsService(baseDirectory, encryptionKey);
settingsService.Load();

// StorageModeを取得
var mode = settingsService.GetStorageMode(); // Local or Database

// 設定値の取得・保存
var theme = settingsService.Get("Ui", "Theme", "Light");
settingsService.Set("Ui", "Theme", "Dark");
settingsService.Save();
```

### ロガー

```csharp
var logger = new FileLogger(logDirectory, appVersion);
logger.Info("AppStart", "アプリケーション起動");
logger.Error("LoginError", "ログイン失敗", exception);
```

### 認証

```csharp
var authService = new AuthenticationService(userRepository);
var user = await authService.LoginAsync("admin", "password");

if (user != null)
{
    Console.WriteLine($"ログイン成功: {user.DisplayName}");
}
```

### エラーハンドリング

```csharp
// セットアップ
var notificationService = new NotificationService();
var logger = new FileExceptionLogger();
var notifier = new ExceptionNotifier(notificationService);
var handler = new GlobalExceptionHandler(notifier, logger);

// 例外処理
try
{
    // 処理
}
catch (Exception ex)
{
    await handler.HandleExceptionAsync(ex, "データ処理");
}
```

### データベース接続（再試行付き）

```csharp
var settings = new DbConnectionSettings
{
    ConnectionString = "Server=localhost;Database=mydb;...",
    MaxRetryCount = 3,
    RetryDelayMs = 1000
};

var factory = new SqlServerConnectionFactory(settings);
using var resilientConnection = new ResilientDbConnection(factory, settings);

// クエリ実行（自動再試行付き）
var result = await resilientConnection.ExecuteAsync(async conn =>
{
    using var command = conn.CreateCommand();
    command.CommandText = "SELECT * FROM Users";
    using var reader = await command.ExecuteReaderAsync();
    return ProcessData(reader);
});
```

## ライセンス

このプロジェクトは開発中のため、ライセンスは未定です。

## 関連ドキュメント

- [仕様書](docs/仕様書.md) - 詳細な機能仕様
- [エラーハンドリング](docs/ErrorHandling.md) - グローバル例外処理と通知システムの使用方法
- [データベース接続](docs/DatabaseConnection.md) - DB接続の抽象化と再試行機能の使用方法
