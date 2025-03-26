using Example.Api.Base;
using Example.Api.Base.Consts;
using Example.NotificationsApi.Database;
using Example.NotificationsApi.Feature.Auth;
using Example.ServiceDefaults;

using FastEndpoints.Security;

using Scalar.AspNetCore;

namespace Example.NotificationsApi;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add service defaults & Aspire client integrations.
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddProblemDetails();

        builder.ConfigureJWT();
        builder.Services.AddAuthorization();
        builder.Services.ConfigureSwagger();
        builder.Services.AddFastEndpoints();
        builder.ConfigureDb<NotificationDbContext>(ConnectionStrings.NotificationDb, DatabaseNames.NotificationDatabase, true);

        builder.Services.AddResilienceEnricher();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddHttpContextAccessor();

        builder.AddSeqEndpoint(ConnectionStrings.Seq);

        builder.ConfigureLocalizer();

        var app = builder.Build();

        app.UseRequestLocalization(CultureConsts.SupportedCultures);

        app.UseJwtRevocation<TokenRevocationMiddleware>()
            .UseAuthentication()
            .UseAuthorization()
            .UseFastEndpoints(static options =>
            {
                options.Endpoints.RoutePrefix = "api";
                options.Errors.UseProblemDetails();
            });

        // Configure the HTTP request pipeline.
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi(static c => c.Path = "/openapi/v1.json");
            app.MapScalarApiReference();
        }

        app.MapDefaultEndpoints();

        await app.RunMigrations<NotificationDbContext>();

        await app.RunAsync();
    }
}
