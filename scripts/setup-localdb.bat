@echo off
REM ========================================
REM DeskAppKit LocalDB セットアップスクリプト
REM ========================================
REM 用途: 開発・テスト環境用のSQL Server LocalDBを自動作成
REM ========================================

echo ========================================
echo DeskAppKit LocalDB セットアップ
echo ========================================
echo.

REM LocalDBインスタンスの確認
echo [1/4] LocalDBインスタンスを確認中...
sqllocaldb info MSSQLLocalDB >nul 2>&1
if %errorlevel% neq 0 (
    echo エラー: LocalDBインスタンス "MSSQLLocalDB" が見つかりません。
    echo.
    echo SQL Server LocalDBをインストールしてください:
    echo https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb
    pause
    exit /b 1
)
echo OK: LocalDBインスタンスが見つかりました

REM LocalDBインスタンスの起動
echo.
echo [2/4] LocalDBインスタンスを起動中...
sqllocaldb start MSSQLLocalDB
if %errorlevel% neq 0 (
    echo 警告: LocalDBインスタンスの起動に失敗しました（既に起動している可能性があります）
) else (
    echo OK: LocalDBインスタンスを起動しました
)

REM スクリプトの実行
echo.
echo [3/4] SQLスクリプトを実行中...
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "%~dp0setup-localdb.sql" -o "%~dp0setup-localdb.log"
if %errorlevel% neq 0 (
    echo エラー: SQLスクリプトの実行に失敗しました
    echo ログファイルを確認してください: %~dp0setup-localdb.log
    pause
    exit /b 1
)
echo OK: SQLスクリプトを実行しました

REM ログファイルの表示
echo.
echo [4/4] セットアップ結果を表示中...
echo.
type "%~dp0setup-localdb.log"

echo.
echo ========================================
echo セットアップ完了
echo ========================================
echo.
echo 接続文字列:
echo Server=(localdb)\MSSQLLocalDB;Database=DeskAppKitDb;Integrated Security=true;TrustServerCertificate=true
echo.
echo テストユーザー:
echo   管理者: admin / Admin123!
echo   一般: testuser / Test123!
echo.
echo ログファイル: %~dp0setup-localdb.log
echo.
pause
