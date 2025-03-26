using Example.AuthApi.Database.Worker.Consumers;
using Example.MassTransit;
using Example.ServiceDefaults;
using Example.Worker.Base;

namespace Example.AuthApi.Database.Worker;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder? builder = WebApplication.CreateSlimBuilder(args);

        builder.WebHost.UseKestrelHttpsConfiguration();

        builder.AddServiceDefaults();

        builder.ConfigureDb<AuthDbContext>(ConnectionStrings.AuthDb, DatabaseNames.AuthDatabase, true);
        builder.ConfigureDb<AuthOutboxDbContext>(ConnectionStrings.AuthOutboxServer, DatabaseNames.AuthOutboxDatabase, false);
        builder.AddSeqEndpoint(ConnectionStrings.Seq);

        builder.AddCustomMassTransit<AuthOutboxDbContext>(false, static options =>
        {
            options.AddConsumer<PasswordRecoveryPersistToDbConsumer>();
            options.AddConsumer<RegisterUserDbConsumer>();
        });

        var app = builder.Build();

        app.MapDefaultEndpoints();

        await app.RunAsync();
    }
}
