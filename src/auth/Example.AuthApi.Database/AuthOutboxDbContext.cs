using Example.Database.Base;
using Example.MassTransit.PasswordRecovery;
using Example.MassTransit.RegisterNewUser;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Example.AuthApi.Database;

public sealed class AuthOutboxDbContext : DbContext
{
    public AuthOutboxDbContext()
    {
    }

    public AuthOutboxDbContext(DbContextOptions<AuthOutboxDbContext> options) : base(options)
    {
    }

    public DbSet<PasswordRecoveryState> PasswordRecoveryStates { get; init; }
    public DbSet<RegisterNewUserModelState> RegisterNewUserModelStates { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        DbContextExtensions.ConfigureBuilder(optionsBuilder);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
