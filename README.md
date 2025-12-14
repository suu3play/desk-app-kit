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

## 開発環境セットアップ

### Windows（LocalDB）

Windows環境では、SQL Server Express LocalDBを使用した開発環境が利用できます。

1. **LocalDBセットアップ（自動）**
   - アプリケーションの設定画面から「ローカルDB環境をセットアップ」ボタンをクリック
   - LocalDBインスタンスの作成、データベース初期化、サンプルデータ投入が自動実行されます

2. **手動セットアップ**
   ```bash
   # LocalDBインスタンスの確認
   sqllocaldb info

   # インスタンスの作成（未作成の場合）
   sqllocaldb create DeskAppKit

   # インスタンスの起動
   sqllocaldb start DeskAppKit
   ```

### Linux / Mac（Docker）

Linux/Mac環境では、Docker Composeを使用してSQL Serverコンテナで開発環境を構築できます。

#### 必要要件
- Docker Desktop (Mac) または Docker Engine (Linux)
- Docker Compose v3.8以降

#### セットアップ手順

1. **SQL Serverコンテナの起動**
   ```bash
   # プロジェクトルートで実行
   docker-compose up -d
   ```

2. **データベース初期化の確認**
   ```bash
   # ログを確認（初期化が完了するまで待機）
   docker-compose logs -f sqlserver-init
   ```

3. **接続設定ファイルの生成**

   **Linux/Mac:**
   ```bash
   ./docker/scripts/generate-bootstrap-config.sh
   ```

   **Windows (PowerShell):**
   ```powershell
   .\docker\scripts\generate-bootstrap-config.ps1
   ```

   このスクリプトは`Data/bootstrap_db.json`を生成します。

4. **アプリケーション起動**
   ```bash
   dotnet run --project src/App.Client/App.Client.csproj
   ```

   Databaseモードで起動し、以下のユーザーでログイン可能：
   - **管理者**: admin / admin123
   - **一般ユーザー**: user / user123

#### Docker環境の接続設定

- **サーバー**: localhost,1433
- **データベース**: DeskAppKitDb
- **ユーザーID**: sa
- **パスワード**: YourStrong!Passw0rd

#### コンテナ管理コマンド

```bash
# コンテナの停止
docker-compose down

# コンテナの停止 + データ削除
docker-compose down -v

# コンテナの再起動
docker-compose restart

# ログの確認
docker-compose logs -f sqlserver
```

#### 注意事項

- **Mac M1/M2**: SQL Server LinuxコンテナはARM非対応のため、Rosetta 2経由での動作となり、パフォーマンスが低下する場合があります
- **ポート競合**: ローカルにSQL Serverがインストール済みの場合、`docker-compose.yml`のポート番号を変更してください
- **セキュリティ**: `bootstrap_db.json`には機密情報が含まれるため、`.gitignore`に追加してバージョン管理から除外してください

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
