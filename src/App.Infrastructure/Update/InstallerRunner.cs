using System.Diagnostics;
using DeskAppKit.Core.Interfaces;

namespace DeskAppKit.Infrastructure.Update;

/// <summary>
/// インストーラー実行クラス
/// </summary>
public class InstallerRunner
{
    private readonly ILogger _logger;

    public InstallerRunner(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// インストーラーを実行してアプリケーションを終了
    /// </summary>
    public void RunInstallerAndExit(string installerPath, bool silent = true)
    {
        try
        {
            if (!File.Exists(installerPath))
            {
                throw new FileNotFoundException("インストーラーが見つかりません", installerPath);
            }

            _logger.Info("InstallerRunner", $"インストーラー実行: {installerPath}");

            var startInfo = new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true,
                Verb = "runas" // 管理者権限で実行
            };

            if (silent)
            {
                startInfo.Arguments = "/SILENT /NORESTART";
            }

            Process.Start(startInfo);

            _logger.Info("InstallerRunner", "インストーラー起動完了。アプリケーションを終了します。");

            // アプリケーションを終了
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            _logger.Error("InstallerRunner", "インストーラーの実行に失敗しました", ex);
            throw;
        }
    }

    /// <summary>
    /// ダウングレード用インストーラーを実行
    /// </summary>
    public void RunDowngradeInstallerAndExit(string installerPath, string targetVersion)
    {
        try
        {
            _logger.Info("InstallerRunner", $"ダウングレード実行: バージョン {targetVersion}");

            // ダウングレードは通常のインストールと同じ
            RunInstallerAndExit(installerPath, silent: true);
        }
        catch (Exception ex)
        {
            _logger.Error("InstallerRunner", "ダウングレードに失敗しました", ex);
            throw;
        }
    }

    /// <summary>
    /// インストール前の検証
    /// </summary>
    public bool ValidateBeforeInstall(string installerPath)
    {
        try
        {
            // ファイル存在チェック
            if (!File.Exists(installerPath))
            {
                _logger.Error("InstallerRunner", "インストーラーが見つかりません");
                return false;
            }

            // ファイルサイズチェック（最低100KB以上）
            var fileInfo = new FileInfo(installerPath);
            if (fileInfo.Length < 100 * 1024)
            {
                _logger.Error("InstallerRunner", "インストーラーファイルのサイズが小さすぎます");
                return false;
            }

            // 拡張子チェック
            if (!installerPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error("InstallerRunner", "インストーラーの拡張子が不正です");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("InstallerRunner", "インストール前の検証に失敗しました", ex);
            return false;
        }
    }
}
