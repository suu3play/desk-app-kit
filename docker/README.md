# Docker環境セットアップガイド

## 概要

このディレクトリには、Linux/Mac環境でDeskAppKitを開発するためのDocker Compose設定が含まれています。

## 構成

```
docker/
├── README.md                          # このファイル
├── init/
│   ├── init.sql                       # データベース初期化スクリプト
│   └── entrypoint.sh                  # エントリポイントスクリプト（未使用）
└── scripts/
    ├── generate-bootstrap-config.sh   # 接続設定生成（Bash）
    └── generate-bootstrap-config.ps1  # 接続設定生成（PowerShell）
```

## クイックスタート

### 1. SQL Serverコンテナの起動

プロジェクトルートディレクトリで以下を実行：

```bash
docker-compose up -d
```

### 2. 初期化の完了を待機

```bash
docker-compose logs -f sqlserver-init
```

以下のメッセージが表示されれば初期化完了：
```
Database initialization completed.
```

### 3. 接続設定ファイルの生成

**Linux/Mac:**
```bash
./docker/scripts/generate-bootstrap-config.sh
```

**Windows (PowerShell):**
```powershell
.\docker\scripts\generate-bootstrap-config.ps1
```

### 4. アプリケーション起動

```bash
dotnet run --project src/App.Client/App.Client.csproj
```

## 接続情報

- **サーバー**: localhost,1433
- **データベース**: DeskAppKitDb
- **ユーザー名**: sa
- **パスワード**: YourStrong!Passw0rd
- **Trust Server Certificate**: True

## サンプルユーザー

LocalDBセットアップと同様に、サンプルデータを投入することで以下のユーザーが利用可能になります：

- **管理者**: admin / admin123
- **一般ユーザー**: user / user123

サンプルデータの投入は、アプリケーションのセットアップUI、またはEF Core Migrationsと併せてDatabaseSeederを実行してください。

## コンテナ管理

### 起動
```bash
docker-compose up -d
```

### 停止
```bash
docker-compose down
```

### 停止（データ削除）
```bash
docker-compose down -v
```

### 再起動
```bash
docker-compose restart
```

### ログ確認
```bash
# SQL Serverのログ
docker-compose logs -f sqlserver

# 初期化スクリプトのログ
docker-compose logs sqlserver-init

# 全コンテナのログ
docker-compose logs -f
```

### コンテナに接続
```bash
docker exec -it deskappkit-sqlserver /bin/bash
```

## トラブルシューティング

### ポート競合

ローカルに既にSQL Serverがインストールされている場合、ポート1433が競合します。

`docker-compose.yml`でポート番号を変更：

```yaml
ports:
  - "1434:1433"  # ホスト側を1434に変更
```

接続設定も変更：
- **サーバー**: localhost,1434

### コンテナが起動しない

```bash
# ログを確認
docker-compose logs sqlserver

# コンテナの状態を確認
docker-compose ps

# 完全にクリーンアップして再起動
docker-compose down -v
docker-compose up -d
```

### データベースに接続できない

1. コンテナが正常に起動しているか確認
   ```bash
   docker-compose ps
   ```

2. ヘルスチェックの状態を確認
   ```bash
   docker-compose ps sqlserver
   ```
   STATUSが`healthy`であることを確認

3. 手動で接続テスト
   ```bash
   docker exec -it deskappkit-sqlserver /opt/mssql-tools/bin/sqlcmd \
     -S localhost -U sa -P YourStrong!Passw0rd -Q "SELECT 1"
   ```

### Mac M1/M2での動作

SQL Server LinuxコンテナはARM非対応のため、Rosetta 2経由で動作します。

- 初回起動が遅い場合があります（数分かかることもあります）
- パフォーマンスが低下する可能性があります
- 代替として、Azure SQL DatabaseやリモートのSQL Serverを使用することを検討してください

## データの永続化

Docker Volumeを使用してデータを永続化しています。

### Volume情報の確認
```bash
docker volume ls
docker volume inspect desk-app-kit_sqlserver-data
```

### Volumeの削除
```bash
# コンテナを停止してVolumeも削除
docker-compose down -v
```

## セキュリティに関する注意

- `docker-compose.yml`のパスワードはデフォルト値です。本番環境では必ず変更してください
- `bootstrap_db.json`には機密情報が含まれるため、バージョン管理から除外されています（`.gitignore`に追加済み）
- 環境変数や`.env`ファイルを使用してパスワードを管理することを推奨します

## カスタマイズ

### パスワードの変更

1. `docker-compose.yml`を編集
   ```yaml
   environment:
     - SA_PASSWORD=YourNewPassword!  # 変更
   ```

2. `docker/init/init.sql`のヘルスチェックも更新（該当する場合）

3. `docker/scripts/generate-bootstrap-config.*`のデフォルトパスワードも更新

### データベース名の変更

1. `docker/init/init.sql`を編集
   ```sql
   CREATE DATABASE YourDatabaseName;
   ```

2. スクリプト実行時にデータベース名を指定
   ```bash
   ./docker/scripts/generate-bootstrap-config.sh ./Data/bootstrap_db.json localhost,1433 YourDatabaseName
   ```

## 参考情報

- [SQL Server Docker公式ドキュメント](https://hub.docker.com/_/microsoft-mssql-server)
- [Docker Composeドキュメント](https://docs.docker.com/compose/)
- [SQL Server on Linux](https://docs.microsoft.com/sql/linux/)
