
using EntityFramework.Exceptions.PostgreSQL;

using Example.Database.Base.Interceptors;
using Example.NotificationsApi.Database;
using Example.NotificationsApi.Feature.Auth;
using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;
using Example.ServiceDefaults.Consts;

using FastEndpoints.Security;
using FastEndpoints.Swagger;

using Microsoft.EntityFrameworkCore;

using NATS.Client.Core;

using Newtonsoft.Json.Converters;

using NJsonSchema.Generation.TypeMappers;

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
        builder.ConfigureDb();

        builder.Services.AddResilienceEnricher();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddHttpContextAccessor();

        builder.AddSeqEndpoint(ConnectionStrings.Seq);
        builder.AddNatsClient(ConnectionStrings.NatsServer, (IServiceProvider _, NatsOpts opt) => opt with { WaitUntilSent = false });

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

        await app.RunMigrations();

        await app.RunAsync();
    }

    private static void ConfigureLocalizer(this WebApplicationBuilder builder)
    {
        builder.Services.AddRequestLocalization(static options =>
        {
            options.SetDefaultCulture(CultureConsts.SupportedCultures[0])
            .AddSupportedCultures(CultureConsts.SupportedCultures)
            .AddSupportedUICultures(CultureConsts.SupportedCultures);

            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        builder.Services.AddJsonLocalization(static options => options.ResourcesPath = "Localization");
    }

    private static void ConfigureJWT(this WebApplicationBuilder builder)
    {
        var jwtSection = builder.Configuration.GetRequiredSection(JwtConfiguration.SectionName);
        builder.Services.Configure<JwtConfiguration>(jwtSection);
        var jwtConfiguration = jwtSection.Get<JwtConfiguration>();

        ArgumentNullException.ThrowIfNull(jwtConfiguration);

        builder.Services.AddAuthenticationJwtBearer(
            options =>
            {
                options.SigningStyle = TokenSigningStyle.Asymmetric;
                options.SigningKey = jwtConfiguration.PublicKey;
                options.KeyIsPemEncoded = true;
            },
            bearer =>
            {
                bearer.TokenValidationParameters.ValidIssuer = jwtConfiguration.Issuer;
                bearer.TokenValidationParameters.ValidAudience = jwtConfiguration.Audience;
            });
    }

    private static void ConfigureSwagger(this IServiceCollection serviceeCollection)
    {
        serviceeCollection.SwaggerDocument(static options =>
        {
            options.DocumentSettings = static settings =>
            {
                settings.Title = "API Docs";
                settings.Version = "v1";

                foreach (var item in typeof(NotificationDbContext).Assembly.ExportedTypes.Where(p => p.IsEnum))
                {
                    settings.SchemaSettings.TypeMappers.Add(new PrimitiveTypeMapper(item, schema =>
                    {
                        schema.Type = NJsonSchema.JsonObjectType.String;
                        schema.Format = "string";

                        foreach (var value in Enum.GetValues(item))
                        {
                            schema.Enumeration.Add(value.ToString());
                        }
                    }));
                }
            };
            options.ShortSchemaNames = true;
            options.NewtonsoftSettings = static settings => settings.Converters.Add(new StringEnumConverter());
        });
    }

    private static void ConfigureDb(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(DatabaseNames.NotificationDatabase);

        builder.AddNpgsqlDbContext<NotificationDbContext>(
            ConnectionStrings.NotificationDb,
            x => x.ConnectionString = connectionString,
            static x =>
            {
                x.AddInterceptors(new AddOrModifyInterceptor(), new ConcurrentInterceptor(), new SoftDeleteInterceptor());
                x.UseNpgsql();
                x.UseExceptionProcessor();
            });
    }

    private static async Task RunMigrations(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await using var authDb = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        await authDb.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
    }
}
