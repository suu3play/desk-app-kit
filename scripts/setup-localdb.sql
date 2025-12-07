-- ========================================
-- DeskAppKit LocalDB セットアップスクリプト
-- ========================================
-- 用途: 開発・テスト環境用のSQL Server LocalDB作成
-- 実行方法: sqlcmd -S (localdb)\MSSQLLocalDB -i setup-localdb.sql
-- ========================================

USE master;
GO

-- データベースが存在する場合は削除（開発用）
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'DeskAppKitDb')
BEGIN
    ALTER DATABASE DeskAppKitDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DeskAppKitDb;
    PRINT 'データベース DeskAppKitDb を削除しました';
END
GO

-- データベース作成
CREATE DATABASE DeskAppKitDb;
GO

PRINT 'データベース DeskAppKitDb を作成しました';
GO

USE DeskAppKitDb;
GO

-- ========================================
-- テーブル作成
-- ========================================

-- Usersテーブル
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    LoginId NVARCHAR(50) NOT NULL UNIQUE,
    DisplayName NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Salt NVARCHAR(255) NOT NULL,
    Role INT NOT NULL DEFAULT 0, -- 0=User, 1=Admin
    AccountStatus INT NOT NULL DEFAULT 0, -- 0=Active, 1=Locked, 2=Disabled
    LockoutCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    LastLoginAt DATETIME2 NULL
);
GO

CREATE INDEX IX_Users_LoginId ON Users(LoginId);
CREATE INDEX IX_Users_AccountStatus ON Users(AccountStatus);
GO

-- Settingsテーブル
CREATE TABLE Settings (
    SettingId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NULL, -- NULL = App設定、値あり = User設定
    Category NVARCHAR(50) NOT NULL,
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Settings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

CREATE INDEX IX_Settings_UserId ON Settings(UserId);
CREATE INDEX IX_Settings_Category_Key ON Settings(Category, SettingKey);
GO

-- Logsテーブル
CREATE TABLE Logs (
    LogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    LogLevel INT NOT NULL, -- 0=Debug, 1=Info, 2=Warn, 3=Error
    Category NVARCHAR(100) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Exception NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX IX_Logs_CreatedAt ON Logs(CreatedAt DESC);
CREATE INDEX IX_Logs_LogLevel ON Logs(LogLevel);
CREATE INDEX IX_Logs_Category ON Logs(Category);
GO

-- SchemaVersionsテーブル
CREATE TABLE SchemaVersions (
    VersionId INT IDENTITY(1,1) PRIMARY KEY,
    Version NVARCHAR(20) NOT NULL UNIQUE,
    AppliedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Description NVARCHAR(500) NULL
);
GO

-- ClientVersionStatusesテーブル
CREATE TABLE ClientVersionStatuses (
    StatusId INT IDENTITY(1,1) PRIMARY KEY,
    Version NVARCHAR(20) NOT NULL UNIQUE,
    Status INT NOT NULL DEFAULT 0, -- 0=Active, 1=UpdateRequired, 2=Deprecated
    ReleaseDate DATETIME2 NOT NULL,
    EndOfSupportDate DATETIME2 NULL,
    Notes NVARCHAR(MAX) NULL
);
GO

PRINT 'テーブルを作成しました';
GO

-- ========================================
-- 初期データ投入
-- ========================================

-- スキーマバージョン
INSERT INTO SchemaVersions (Version, Description)
VALUES ('1.0.0', '初期スキーマ');
GO

-- テスト用管理者ユーザー
-- ログインID: admin
-- パスワード: Admin123!
-- BCrypt WorkFactor=12 でハッシュ化済み
INSERT INTO Users (UserId, LoginId, DisplayName, PasswordHash, Salt, Role, AccountStatus, LockoutCount, CreatedAt)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'admin',
    'テスト管理者',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5rvJ7qqJ2qOau', -- Admin123!
    'test-salt-value-admin',
    1, -- Admin
    0, -- Active
    0,
    GETUTCDATE()
);
GO

-- テスト用一般ユーザー
-- ログインID: testuser
-- パスワード: Test123!
INSERT INTO Users (UserId, LoginId, DisplayName, PasswordHash, Salt, Role, AccountStatus, LockoutCount, CreatedAt)
VALUES (
    '00000000-0000-0000-0000-000000000002',
    'testuser',
    'テストユーザー',
    '$2a$12$R9h/cIPz0gi.URNNX3kh2OPST9/PgBkqquzi.Ss7KQpOtsOihbWKi', -- Test123!
    'test-salt-value-user',
    0, -- User
    0, -- Active
    0,
    GETUTCDATE()
);
GO

-- App設定のデフォルト値
INSERT INTO Settings (UserId, Category, SettingKey, SettingValue, CreatedAt)
VALUES
    (NULL, 'System', 'StorageMode', 'Database', GETUTCDATE()),
    (NULL, 'System', 'AppVersion', '1.0.0', GETUTCDATE()),
    (NULL, 'System', 'EncryptionEnabled', 'true', GETUTCDATE()),
    (NULL, 'Logging', 'LogLevel', 'Info', GETUTCDATE()),
    (NULL, 'Logging', 'LogRetentionDays', '30', GETUTCDATE()),
    (NULL, 'UI', 'Theme', 'Light', GETUTCDATE()),
    (NULL, 'UI', 'Language', 'ja-JP', GETUTCDATE());
GO

-- クライアントバージョン
INSERT INTO ClientVersionStatuses (Version, Status, ReleaseDate, Notes)
VALUES
    ('1.0.0', 0, GETUTCDATE(), '初期リリース');
GO

PRINT '初期データを投入しました';
GO

-- ========================================
-- 確認クエリ
-- ========================================

PRINT '';
PRINT '========================================';
PRINT 'セットアップ完了';
PRINT '========================================';
PRINT '';

SELECT 'データベース情報' AS Category,
       DB_NAME() AS DatabaseName,
       CAST(@@VERSION AS NVARCHAR(100)) AS SQLServerVersion;

PRINT '';
PRINT 'テーブル一覧:';
SELECT TABLE_NAME,
       (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = T.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES T
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

PRINT '';
PRINT 'テストユーザー一覧:';
SELECT LoginId, DisplayName,
       CASE Role WHEN 0 THEN 'User' WHEN 1 THEN 'Admin' END AS RoleName,
       CASE AccountStatus WHEN 0 THEN 'Active' WHEN 1 THEN 'Locked' WHEN 2 THEN 'Disabled' END AS StatusName
FROM Users;

PRINT '';
PRINT '========================================';
PRINT '接続文字列例:';
PRINT 'Server=(localdb)\MSSQLLocalDB;Database=DeskAppKitDb;Integrated Security=true;TrustServerCertificate=true';
PRINT '';
PRINT 'テストユーザー:';
PRINT '  管理者: admin / Admin123!';
PRINT '  一般: testuser / Test123!';
PRINT '========================================';
GO
