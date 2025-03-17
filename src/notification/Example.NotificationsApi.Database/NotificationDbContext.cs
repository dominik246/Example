using Example.Database.Base;
using Example.NotificationsApi.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace Example.NotificationsApi.Database;

public sealed class NotificationDbContext : DbContext
{
    public NotificationDbContext()
    {
    }

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; init; }
    public DbSet<UserNotification> UserNotifications { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        DbContextExtensions.ConfigureBuilder(optionsBuilder);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        DbContextExtensions.CreateModel(modelBuilder);

        modelBuilder.Entity<UserNotification>().HasKey(p => new { p.NotificationId, p.RecieverId });
    }
}
