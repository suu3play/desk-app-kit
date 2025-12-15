-- DeskAppKit 初期データベースセットアップスクリプト
-- Docker環境用

USE master;
GO

-- データベースが存在しない場合のみ作成
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DeskAppKitDb')
BEGIN
    CREATE DATABASE DeskAppKitDb;
    PRINT 'Database DeskAppKitDb created successfully.';
END
ELSE
BEGIN
    PRINT 'Database DeskAppKitDb already exists.';
END
GO

USE DeskAppKitDb;
GO

-- EF Core Migrationsが実行されるため、テーブル作成は不要
-- アプリケーション起動時に自動的にMigrationsが適用されます

PRINT 'Database initialization completed.';
GO
