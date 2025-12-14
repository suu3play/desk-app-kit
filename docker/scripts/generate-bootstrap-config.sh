#!/bin/bash
# DeskAppKit bootstrap_db.json 生成スクリプト (Docker環境用)
# Linux/Mac用

# デフォルト設定
OUTPUT_PATH="${1:-./Data/bootstrap_db.json}"
SERVER="${2:-localhost,1433}"
DATABASE="${3:-DeskAppKitDb}"
USER_ID="${4:-sa}"
PASSWORD="${5:-YourStrong!Passw0rd}"

# 出力ディレクトリを作成
OUTPUT_DIR=$(dirname "$OUTPUT_PATH")
mkdir -p "$OUTPUT_DIR"

# bootstrap_db.jsonの内容を作成
cat > "$OUTPUT_PATH" << EOF
{
  "Server": "$SERVER",
  "Database": "$DATABASE",
  "UserId": "$USER_ID",
  "Password": "$PASSWORD",
  "IntegratedSecurity": false,
  "Port": 1433,
  "TrustServerCertificate": true
}
EOF

echo -e "\033[0;32mbootstrap_db.json generated successfully at: $OUTPUT_PATH\033[0m"
echo ""
echo -e "\033[0;36mConnection settings:\033[0m"
echo "  Server: $SERVER"
echo "  Database: $DATABASE"
echo "  User ID: $USER_ID"
echo "  Password: $PASSWORD"
echo ""
echo -e "\033[0;33mWARNING: This file contains sensitive information. Do not commit it to version control!\033[0m"
