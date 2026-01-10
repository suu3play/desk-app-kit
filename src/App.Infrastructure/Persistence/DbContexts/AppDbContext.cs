using Microsoft.EntityFrameworkCore;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Persistence.DbContexts;

/// <summary>
/// アプリケーションDbContext
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<SchemaVersion> SchemaVersions { get; set; }
    public DbSet<ClientVersionStatus> ClientVersionStatuses { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DBプロバイダーを検出
        var isSqlite = Database.IsSqlite();
        var isSqlServer = Database.IsSqlServer();

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.LoginId).IsUnique();
            entity.Property(e => e.LoginId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Salt).HasMaxLength(200).IsRequired();

            // SQLiteの場合、GuidをTEXT型として扱う
            if (isSqlite)
            {
                entity.Property(e => e.UserId).HasConversion<string>();
            }
        });

        // Settings
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.SettingId);
            entity.HasIndex(e => new { e.ScopeType, e.UserId, e.Category, e.Key }).IsUnique();
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Key).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DataType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            if (isSqlite)
            {
                entity.Property(e => e.SettingId).HasConversion<string>();
                entity.Property(e => e.UserId).HasConversion<string>();
            }
        });

        // Logs
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.LogId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Level);
            entity.Property(e => e.Logger).HasMaxLength(200).IsRequired();
            entity.Property(e => e.MachineName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AppVersion).HasMaxLength(50).IsRequired();

            if (isSqlite)
            {
                entity.Property(e => e.LogId).HasConversion<string>();
            }
        });

        // SchemaVersion
        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.SchemaVersionId);
            entity.HasIndex(e => e.Version).IsUnique();
            entity.HasIndex(e => e.IsCurrent);
            entity.Property(e => e.Version).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MigrationScriptUpPath).HasMaxLength(500);
            entity.Property(e => e.MigrationScriptDownPath).HasMaxLength(500);

            if (isSqlite)
            {
                entity.Property(e => e.SchemaVersionId).HasConversion<string>();
            }
        });

        // ClientVersionStatus
        modelBuilder.Entity<ClientVersionStatus>(entity =>
        {
            entity.HasKey(e => e.ClientVersionStatusId);
            entity.HasIndex(e => e.MachineName);
            entity.Property(e => e.MachineName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CurrentAppVersion).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CurrentSchemaVersion).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastUpdateErrorMessage).HasMaxLength(1000);

            if (isSqlite)
            {
                entity.Property(e => e.ClientVersionStatusId).HasConversion<string>();
            }
        });

        // Notifications
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);

            if (isSqlite)
            {
                entity.Property(e => e.Id).HasConversion<string>();
                entity.Property(e => e.UserId).HasConversion<string>();
            }
        });

        // AuditLogs
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.Property(e => e.UserName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.MachineName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);

            if (isSqlite)
            {
                entity.Property(e => e.Id).HasConversion<string>();
                entity.Property(e => e.UserId).HasConversion<string>();
            }
        });
    }
}
