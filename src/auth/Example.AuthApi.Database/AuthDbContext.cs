using Microsoft.EntityFrameworkCore;

using Example.AuthApi.Database.Models;
using Example.Database.Base;

namespace Example.AuthApi.Database;

public sealed class AuthDbContext : DbContext
{
    public AuthDbContext()
    {
    }

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<UserToken> UserTokens { get; init; }
    public DbSet<User> Users { get; init; } = default!;
    public DbSet<UserPasswordRestore> UserPasswordRestores { get; init; }
    public DbSet<UserEmailConfirm> UserEmailConfirms { get; init; }
    public DbSet<Group> Groups { get; init; }
    public DbSet<UserGroup> UserGroups { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        DbContextExtensions.ConfigureBuilder(optionsBuilder);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        DbContextExtensions.CreateModel(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(p => p.Email).IsUnique();
        modelBuilder.Entity<UserGroup>().HasKey(x => new { x.UserId, x.GroupId });
        modelBuilder.Entity<Group>().HasIndex(x => x.Name).IsUnique();

        var group = new Group
        {
            Name = "Admin",
            Id = Guid.Parse("01958cf3-d0bb-7509-80b6-4a15b06ad596"),
            IsDeletable = false,
            IsAdminGroup = true
        };

        var user = new User
        {
            Id = Guid.Parse("019587b0-955a-72ad-a5eb-09442f53a85d"),
            Email = "admin@example.hr",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAELyuFf3ULbO89ih8KD4YGU4NN5+o0cg7H1XmrO+aUULcr3CuN5RfyM+ZcbYJDJNx4A==", // Password123%
        };

        var userGroup = new UserGroup
        {
            GroupId = group.Id,
            UserId = user.Id,
        };

        modelBuilder.Entity<Group>().HasData(group);
        modelBuilder.Entity<User>().HasData(user);
        modelBuilder.Entity<UserGroup>().HasData(userGroup);
    }
}
