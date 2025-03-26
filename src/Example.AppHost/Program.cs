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
        var mq = builder.AddRabbitMQ();

        var authApi = builder.ConfigureApi<Projects.Example_AuthApi>(ProjectNames.AuthApi)
            .WithEnvironmentSection(JwtConfiguration.SectionName, EmailConfiguration.SectionName)
            .WithCache(AuthCacheConfiguration.SectionName, ConnectionStrings.AuthCache, out var authCache)
            .WithPostgresDb(ConnectionStrings.AuthDb, DatabaseNames.AuthDatabase, DatabaseConfiguration.AuthSectionName, out var authSqlServer, out var authDb)
            .WithDbWorker<Projects.Example_AuthApi_Database_Worker>(authDb, ProjectNames.AuthDbWorker, seq, mq, out var authDbWorker)
            .WithPostgresDb(ConnectionStrings.AuthOutboxServer, DatabaseNames.AuthOutboxDatabase, DatabaseConfiguration.AuthOutboxSectionName, out var outboxServer, out var outboxDb);

        redisInsight.WithReference(authCache);
        redisCommander.WithReference(authCache);
        dbGate.WithReference(authSqlServer);
        dbGate.WithReference(outboxServer);
        authApi.WithReference(mq).WaitFor(mq);
        authDbWorker.WithReference(mq).WaitFor(mq);
        authApi.WithReference(seq).WaitFor(seq);
        authDbWorker.WithReference(authDb).WaitFor(authDb);
        authDbWorker.WithReference(outboxDb).WaitFor(outboxDb);

        var emailService = builder.ConfigureApi<Projects.Example_EmailService>(ProjectNames.EmailService)
            .WithEnvironmentSection(EmailConfiguration.SectionName);

        emailService.WithReference(mq).WaitFor(mq);
        emailService.WithReference(seq).WaitFor(seq);
        emailService.WithReference(outboxDb).WaitFor(outboxDb);

        var notificationApi = builder.ConfigureApi<Projects.Example_NotificationsApi>(ProjectNames.NotificationApi)
            .WithEnvironmentSection(JwtConfiguration.SectionName)
            .WithPostgresDb(ConnectionStrings.NotificationDb, DatabaseNames.NotificationDatabase, DatabaseConfiguration.NotificationSectionName, out var notificationSqlServer, out _);

        notificationApi.WithReference(mq).WaitFor(mq);
        notificationApi.WithReference(seq).WaitFor(seq);
        dbGate.WithReference(notificationSqlServer);

        var app = builder.Build();
        await app.RunAsync();
    }
}
