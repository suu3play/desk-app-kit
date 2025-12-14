# DeskAppKit bootstrap_db.json 生成スクリプト (Docker環境用)
# Windows PowerShell用

param(
    [string]$OutputPath = ".\Data\bootstrap_db.json",
    [string]$Server = "localhost,1433",
    [string]$Database = "DeskAppKitDb",
    [string]$UserId = "sa",
    [string]$Password = "YourStrong!Passw0rd"
)

# 出力ディレクトリを作成
$outputDir = Split-Path -Parent $OutputPath
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# 暗号化キー（本番環境では安全に管理してください）
$encryptionKey = "YourSecureEncryptionKey32Chars!!"

# bootstrap_db.jsonの内容を作成
$config = @{
    Server = $Server
    Database = $Database
    UserId = $UserId
    Password = $Password
    IntegratedSecurity = $false
    Port = 1433
    TrustServerCertificate = $true
}

# 簡易的な暗号化（実際のアプリと同じロジックを使用する必要があります）
# ここでは平文で保存していますが、実際にはアプリケーション側の暗号化を使用してください
$json = $config | ConvertTo-Json -Depth 10

# ファイルに保存
$json | Out-File -FilePath $OutputPath -Encoding UTF8

Write-Host "bootstrap_db.json generated successfully at: $OutputPath" -ForegroundColor Green
Write-Host ""
Write-Host "Connection settings:" -ForegroundColor Cyan
Write-Host "  Server: $Server"
Write-Host "  Database: $Database"
Write-Host "  User ID: $UserId"
Write-Host "  Password: $Password"
Write-Host ""
Write-Host "WARNING: This file contains sensitive information. Do not commit it to version control!" -ForegroundColor Yellow
