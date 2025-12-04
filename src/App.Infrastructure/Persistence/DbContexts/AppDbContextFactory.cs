using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeskAppKit.Infrastructure.Persistence.DbContexts;

/// <summary>
/// デザインタイム用DbContextファクトリ（マイグレーション生成用）
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // デザインタイム用の一時的な接続文字列
        // 実際の接続文字列はbootstrap_db.jsonから取得される
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=DeskAppKitDb;Integrated Security=True;TrustServerCertificate=True;",
            b => b.MigrationsAssembly("App.Infrastructure"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
