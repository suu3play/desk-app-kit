#!/bin/bash

# SQL Serverの起動を待機
echo "Waiting for SQL Server to start..."
sleep 30s

# 初期化スクリプトを実行
echo "Running initialization script..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -i /docker-entrypoint-initdb.d/init.sql

echo "Initialization completed."
