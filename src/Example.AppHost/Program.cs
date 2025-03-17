using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;

namespace Example.AppHost;

public static class Program
{
    public static async Task Main(params string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var redisInsight = builder.ConfigureRedisInsight();
        var redisCommander = builder.ConfigureRedisCommander();
        var dbGate = builder.AddDbGate();
        var seq = builder.ConfigureSeq();
        var nats = builder.ConfigureNats();

        var authApi = builder.ConfigureApi<Projects.Example_AuthApi>(ProjectNames.AuthApi)
            .WithEnvironmentSection(JwtConfiguration.SectionName, EmailConfiguration.SectionName)
            .WithCache(AuthCacheConfiguration.SectionName, ConnectionStrings.AuthCache, out var authCache)
            .WithPostgresDb(ConnectionStrings.AuthDb, DatabaseNames.AuthDatabase, DatabaseConfiguration.AuthSectionName, out var authSqlServer, out _);

        redisInsight.WithReference(authCache);
        redisCommander.WithReference(authCache);
        dbGate.WithReference(authSqlServer);
        authApi.WithReference(nats).WaitFor(nats);
        authApi.WithReference(seq).WaitFor(seq);

        var emailService = builder.ConfigureApi<Projects.Example_EmailService>(ProjectNames.EmailService)
            .WithEnvironmentSection(EmailConfiguration.SectionName);

        emailService.WithReference(nats).WaitFor(nats);
        emailService.WithReference(seq).WaitFor(seq);

        var notificationApi = builder.ConfigureApi<Projects.Example_NotificationsApi>(ProjectNames.NotificationApi)
            .WithEnvironmentSection(JwtConfiguration.SectionName)
            .WithPostgresDb(ConnectionStrings.NotificationDb, DatabaseNames.NotificationDatabase, DatabaseConfiguration.NotificationSectionName, out var notificationSqlServer, out _);

        notificationApi.WithReference(nats).WaitFor(nats);
        notificationApi.WithReference(seq).WaitFor(seq);
        dbGate.WithReference(notificationSqlServer);

        var app = builder.Build();
        await app.RunAsync();
    }
}
