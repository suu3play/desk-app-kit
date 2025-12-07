# DeskAppKit サンプル設定ファイル

このディレクトリには、DeskAppKitの設定ファイルのサンプルが含まれています。

## ファイル一覧

### 1. settings_app.json.sample
**用途**: アプリケーション全体の設定（全ユーザー共通）

**配置場所**:
```
%LOCALAPPDATA%\DeskAppKit\Data\settings_app.json
```

**重要**: 実際のファイルは暗号化されて保存されます。このサンプルは構造の参考用です。

**設定項目**:

| カテゴリ | キー | 説明 | デフォルト値 |
|---------|-----|------|------------|
| System | StorageMode | ストレージモード（Local/Database） | Local |
| System | AppVersion | アプリケーションバージョン | 1.0.0 |
| System | EncryptionEnabled | 暗号化有効フラグ | true |
| Logging | LogLevel | ログレベル（Debug/Info/Warn/Error） | Info |
| Logging | LogRetentionDays | ログ保持日数 | 30 |
| Logging | LogDirectory | ログ出力ディレクトリ | %LOCALAPPDATA%\DeskAppKit\Logs |
| Update | AutoUpdateEnabled | 自動更新有効フラグ | false |
| Update | UpdateCheckInterval | 更新チェック間隔（秒） | 86400 |
| Update | UpdateServerUrl | 更新サーバーURL | - |
| UI | Theme | テーマ（Light/Dark） | Light |
| UI | Language | 言語コード | ja-JP |
| UI | WindowWidth | ウィンドウ幅 | 1024 |
| UI | WindowHeight | ウィンドウ高さ | 768 |

### 2. settings_user.json.sample
**用途**: ユーザー別の設定（各ユーザー個別）

**配置場所**:
```
%LOCALAPPDATA%\DeskAppKit\Data\settings_user.json
```

**重要**: 実際のファイルは暗号化されて保存されます。このサンプルは構造の参考用です。

**設定項目**:

| カテゴリ | キー | 説明 | デフォルト値 |
|---------|-----|------|------------|
| UI | Theme | ユーザー別テーマ | Dark |
| UI | Language | ユーザー別言語 | ja-JP |
| UI | FontSize | フォントサイズ | 14 |
| Preferences | ShowWelcomeScreen | ウェルカム画面表示 | false |
| Preferences | AutoSaveEnabled | 自動保存有効 | true |
| Preferences | AutoSaveInterval | 自動保存間隔（秒） | 300 |
| Dashboard | DefaultView | デフォルトビュー | Summary |
| Dashboard | RefreshInterval | 更新間隔（秒） | 60 |
| Dashboard | ShowNotifications | 通知表示 | true |
| Export | DefaultFormat | デフォルトエクスポート形式 | CSV |
| Export | DefaultEncoding | デフォルトエンコーディング | UTF-8 |
| Export | IncludeHeaders | ヘッダー行を含む | true |

### 3. bootstrap_db.json.sample
**用途**: データベース接続情報（Databaseモード使用時）

**配置場所**:
```
%LOCALAPPDATA%\DeskAppKit\Data\bootstrap_db.json
```

**重要**:
- 実際のファイルは**暗号化**されて保存されます
- パスワードなどの機密情報が含まれるため、手動作成は推奨されません
- アプリケーションの設定画面から作成してください

**設定項目**:

| キー | 説明 | 例 |
|-----|------|---|
| Server | SQL Serverのサーバー名 | localhost, .\SQLEXPRESS, 192.168.1.100 |
| Port | ポート番号 | 1433 |
| Database | データベース名 | DeskAppKitDb |
| UserId | ユーザーID（SQL Server認証時） | sa |
| Password | パスワード（SQL Server認証時） | P@ssw0rd |
| IntegratedSecurity | Windows認証を使用 | true/false |

**認証モード**:

**Windows認証の場合**:
```json
{
  "Server": "localhost",
  "Port": 1433,
  "Database": "DeskAppKitDb",
  "UserId": "",
  "Password": "",
  "IntegratedSecurity": true
}
```

**SQL Server認証の場合**:
```json
{
  "Server": "localhost",
  "Port": 1433,
  "Database": "DeskAppKitDb",
  "UserId": "sa",
  "Password": "YourPassword",
  "IntegratedSecurity": false
}
```

## 使用方法

### 初回セットアップ

1. **アプリケーションを起動**
   - 初回起動時に自動的にDataディレクトリが作成されます
   - Localモードで起動します（認証機能なし・デモモード）

2. **Localモード（デフォルト）での起動**
   - ログイン画面をスキップして、直接メイン画面が表示されます
   - ローカルユーザー（LoginId: `local`、DisplayName: `ローカルユーザー`）として自動ログイン
   - すべての機能が利用可能（ただし認証・ユーザー管理を除く）
   - 設定ファイルは自動的に作成されます

3. **Databaseモードに切り替える場合**
   - アプリケーションのメニューから「設定」を開く
   - StorageModeを「Database」に変更
   - データベース接続情報を入力
   - 「接続テスト」で確認
   - 「保存」をクリック
   - アプリケーションを再起動

### 設定の編集

**方法1: アプリケーションUI経由（推奨）**
- アプリケーションの設定画面から変更
- 自動的に暗号化されて保存されます

**方法2: 直接編集（非推奨）**
- アプリケーションを終了
- 設定ファイルを手動編集
- ただし、暗号化されているため解読・編集は困難
- 推奨されません

### 設定のバックアップ

**Localモード**:
```
%LOCALAPPDATA%\DeskAppKit\Data\
```
フォルダ全体をバックアップ

**Databaseモード**:
SQL Serverのバックアップ機能を使用
```sql
BACKUP DATABASE DeskAppKitDb
TO DISK = 'D:\Backup\DeskAppKitDb.bak';
```

### 設定のリストア

**Localモード**:
1. アプリケーションを終了
2. バックアップファイルを元の場所に上書きコピー
3. アプリケーションを起動

**Databaseモード**:
```sql
RESTORE DATABASE DeskAppKitDb
FROM DISK = 'D:\Backup\DeskAppKitDb.bak'
WITH REPLACE;
```

## トラブルシューティング

### 設定ファイルが破損した場合

1. アプリケーションを終了
2. 設定ファイルを削除
   ```
   %LOCALAPPDATA%\DeskAppKit\Data\settings_*.json
   ```
3. アプリケーションを再起動
4. 初期設定から再作成

### データベース接続に失敗する場合

1. SQL Serverサービスが起動しているか確認
   ```
   サービス > SQL Server (SQLEXPRESS)
   ```
2. 接続情報が正しいか確認
3. ファイアウォール設定を確認
4. ログファイルでエラー詳細を確認
   ```
   %LOCALAPPDATA%\DeskAppKit\Logs\app_yyyyMMdd.log
   ```

## セキュリティに関する注意

### 暗号化について

- **settings_app.json**: AES-256で暗号化
- **settings_user.json**: AES-256で暗号化
- **bootstrap_db.json**: AES-256で暗号化

暗号化キーはアプリケーション内にハードコードされています。

### パスワード管理

- データベースパスワードは`bootstrap_db.json`に暗号化保存
- ユーザーパスワードはBCrypt（WorkFactor=12）でハッシュ化してDBに保存
- 平文パスワードは保存されません

### ベストプラクティス

1. **bootstrap_db.json**をバージョン管理システムにコミットしない
2. バックアップファイルは安全な場所に保管
3. SQL Server認証を使用する場合は強力なパスワードを設定
4. 定期的にバックアップを取得

## 参考資料

- [セットアップガイド](../docs/セットアップガイド.md)
- [運用マニュアル](../docs/運用マニュアル.md)
- [実装状況](../docs/実装状況.md)

## サポート

問題が発生した場合は、以下を確認してください：

1. ログファイル: `%LOCALAPPDATA%\DeskAppKit\Logs\app_yyyyMMdd.log`
2. トラブルシューティングガイド: [運用マニュアル](../docs/運用マニュアル.md#トラブルシューティング)
3. GitHub Issues: [リポジトリURL]/issues
