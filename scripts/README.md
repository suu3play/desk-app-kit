# DeskAppKit スクリプト

このディレクトリには、開発・テスト環境のセットアップ用スクリプトが含まれています。

## ファイル一覧

### 1. setup-localdb.sql
**用途**: SQL Server LocalDB用のデータベースセットアップスクリプト

**機能**:
- データベース `DeskAppKitDb` の作成
- テーブル作成（Users, Settings, Logs, SchemaVersions, ClientVersionStatuses）
- 初期データ投入
- テストユーザーの作成

**実行方法**:

```bash
# 方法1: setup-localdb.bat を実行（推奨）
setup-localdb.bat

# 方法2: sqlcmd コマンドで直接実行
sqlcmd -S "(localdb)\MSSQLLocalDB" -i setup-localdb.sql
```

### 2. setup-localdb.bat
**用途**: LocalDBセットアップの自動化バッチファイル

**機能**:
- LocalDBインスタンスの存在確認
- LocalDBインスタンスの起動
- setup-localdb.sql の実行
- セットアップ結果の表示

**実行方法**:

```bash
# コマンドプロンプトまたはPowerShellから実行
cd d:\自己開発\desk-app-kit\scripts
setup-localdb.bat
```

## LocalDBのセットアップ手順

### 前提条件

1. **SQL Server LocalDBのインストール**
   - Visual Studio 2022がインストールされている場合は、LocalDBも自動的にインストールされています
   - または、[SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)からダウンロードしてインストール

2. **LocalDBインスタンスの確認**
   ```bash
   sqllocaldb info
   # 出力例: MSSQLLocalDB
   ```

### セットアップ実行

1. **バッチファイルを実行**
   ```bash
   cd d:\自己開発\desk-app-kit\scripts
   setup-localdb.bat
   ```

2. **実行結果の確認**
   - スクリプトが正常に実行されると、以下の情報が表示されます：
     - データベース情報
     - テーブル一覧
     - テストユーザー一覧
     - 接続文字列

3. **接続文字列をコピー**
   ```
   Server=(localdb)\MSSQLLocalDB;Database=DeskAppKitDb;Integrated Security=true;TrustServerCertificate=true
   ```

### テストユーザー

セットアップスクリプトは以下のテストユーザーを自動的に作成します：

| ログインID | パスワード | 権限 | 説明 |
|-----------|----------|------|------|
| admin | Admin123! | 管理者 | テスト用管理者アカウント |
| testuser | Test123! | 一般ユーザー | テスト用一般アカウント |

**注意**: これらは開発・テスト専用のアカウントです。本番環境では使用しないでください。

## アプリケーションからの接続

### 1. bootstrap_db.json の作成

LocalDBに接続するために、`bootstrap_db.json` を作成します。

**手動作成する場合**（上級者向け）:

1. サンプルファイルをコピー
   ```bash
   copy samples\bootstrap_db.json.sample %LOCALAPPDATA%\DeskAppKit\Data\bootstrap_db.json
   ```

2. ファイルを編集
   ```json
   {
     "Server": "(localdb)\\MSSQLLocalDB",
     "Port": 1433,
     "Database": "DeskAppKitDb",
     "UserId": "",
     "Password": "",
     "IntegratedSecurity": true
   }
   ```

**アプリケーションから作成する場合**（推奨）:

1. アプリケーションをLocalモードで起動
2. メインメニューから「設定」を選択
3. StorageModeを「Database」に変更
4. DB接続情報を入力:
   - サーバー名: `(localdb)\MSSQLLocalDB`
   - データベース名: `DeskAppKitDb`
   - Windows認証を使用: チェック
5. 「接続テスト」をクリック
6. 「保存」をクリック
7. アプリケーションを再起動

### 2. マイグレーションの適用（必要に応じて）

スキーマが変更された場合は、マイグレーションを適用します：

```bash
cd src/App.Infrastructure
dotnet ef database update
```

## トラブルシューティング

### LocalDBインスタンスが見つからない

**エラー**: `LocalDBインスタンス "MSSQLLocalDB" が見つかりません`

**解決方法**:
1. Visual Studio Installerを起動
2. 「変更」→「個別のコンポーネント」
3. 「SQL Server Express 2019 LocalDB」にチェック
4. インストール

### LocalDBインスタンスが起動しない

**エラー**: `LocalDBインスタンスの起動に失敗しました`

**解決方法**:
```bash
# 既存インスタンスの停止
sqllocaldb stop MSSQLLocalDB

# インスタンスの削除
sqllocaldb delete MSSQLLocalDB

# 新しいインスタンスの作成
sqllocaldb create MSSQLLocalDB

# インスタンスの起動
sqllocaldb start MSSQLLocalDB

# セットアップスクリプトを再実行
setup-localdb.bat
```

### データベースが既に存在する

**エラー**: `データベース 'DeskAppKitDb' は既に存在します`

**解決方法**:

スクリプトは既存のデータベースを自動的に削除して再作成します。データを保持したい場合は、以下のコマンドでバックアップしてください：

```sql
-- SSMSまたはsqlcmdで実行
BACKUP DATABASE DeskAppKitDb
TO DISK = 'C:\Backup\DeskAppKitDb.bak';
```

### 接続テストが失敗する

**エラー**: アプリケーションから接続テストが失敗する

**確認事項**:
1. LocalDBインスタンスが起動しているか確認
   ```bash
   sqllocaldb info MSSQLLocalDB
   # State: Running
   ```

2. データベースが存在するか確認
   ```bash
   sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases WHERE name='DeskAppKitDb'"
   ```

3. 接続文字列が正しいか確認
   - サーバー名: `(localdb)\MSSQLLocalDB` （バックスラッシュは2つ）
   - IntegratedSecurity: `true`

## 参考資料

- [SQL Server Express LocalDB](https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Entity Framework Core マイグレーション](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [セットアップガイド](../docs/セットアップガイド.md)
- [運用マニュアル](../docs/運用マニュアル.md)

## サポート

問題が発生した場合は、以下を確認してください：

1. ログファイル: `setup-localdb.log`
2. アプリケーションログ: `%LOCALAPPDATA%\DeskAppKit\Logs\app_yyyyMMdd.csv`
3. GitHub Issues: [リポジトリURL]/issues
