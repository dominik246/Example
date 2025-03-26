using Example.Api.Base;
using Example.Api.Base.Consts;
using Example.AuthApi.Database;
using Example.AuthApi.Feature.Auth;
using Example.AuthApi.Feature.Auth.Dtos;
using Example.AuthApi.Feature.Auth.Handlers;
using Example.MassTransit;
using Example.MassTransit.PasswordRecovery;
using Example.MassTransit.RegisterNewUser;
using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;

using FastEndpoints.Security;

using MassTransit;

using NJsonSchema;

using Scalar.AspNetCore;

using System.Text.Json.Serialization;

namespace Example.AuthApi;

public static class Program
{
    public static async Task Main(params string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add service defaults & Aspire client integrations.
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddProblemDetails();

        builder.ConfigureJWT();
        builder.Services.AddAuthorization();
        builder.Services.ConfigureSwagger((typeof(EmailAddress), JsonObjectType.String), (typeof(AuthPassword), JsonObjectType.String));
        builder.Services.AddFastEndpoints();
        builder.ConfigureDb<AuthDbContext>(ConnectionStrings.AuthDb, DatabaseNames.AuthDatabase, true);
        builder.ConfigureDb<AuthOutboxDbContext>(ConnectionStrings.AuthOutboxServer, DatabaseNames.AuthOutboxDatabase, false);

        builder.Services.AddResilienceEnricher();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddHttpContextAccessor();

        builder.AddSeqEndpoint(ConnectionStrings.Seq);
        builder.AddRedisClient(ConnectionStrings.AuthCache);

        builder.AddCustomMassTransit<AuthOutboxDbContext>(true, static options =>
        {
            options.AddSagaStateMachine<RegisterNewUserStateMachine, RegisterNewUserModelState>()
                .EntityFrameworkRepository(static x =>
                {
                    x.ExistingDbContext<AuthOutboxDbContext>();
                    x.UsePostgres();
                });

            options.AddSagaStateMachine<PasswordRecoveryStateMachine, PasswordRecoveryState>()
                .EntityFrameworkRepository(static x =>
                {
                    x.ExistingDbContext<AuthOutboxDbContext>();
                    x.UsePostgres();
                });

            options.AddConsumer<RegisterUserCacheConsumer>();
            options.AddConsumer<PersistToCacheConsumer>();
        });

        builder.ConfigureLocalizer();

        builder.Services.Configure<AuthCacheConfiguration>(builder.Configuration.GetRequiredSection(AuthCacheConfiguration.SectionName));

        var app = builder.Build();

        app.UseRequestLocalization(CultureConsts.SupportedCultures);

        app.UseJwtRevocation<TokenRevocationMiddleware>()
            .UseAuthentication()
            .UseAuthorization()
            .UseFastEndpoints(static options =>
            {
                options.Endpoints.RoutePrefix = "api";
                options.Errors.UseProblemDetails();
                options.Serializer.Options.Converters.Add(new AuthPasswordJsonConverter2());
                options.Serializer.Options.Converters.Add(new EmailAddressJsonConverter2());
                options.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
            });

        // Configure the HTTP request pipeline.
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi(static c => c.Path = "/openapi/v1.json");
            app.MapScalarApiReference();
        }

        app.MapDefaultEndpoints();

        await app.RunMigrations<AuthDbContext>();
        await app.RunMigrations<AuthOutboxDbContext>();

        await app.RunAsync();
    }
}
