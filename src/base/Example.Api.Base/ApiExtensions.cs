using EntityFramework.Exceptions.PostgreSQL;

using Example.Api.Base.Consts;
using Example.Database.Base.BaseModels;
using Example.Database.Base.Interceptors;
using Example.ServiceDefaults.Configuration;

using FastEndpoints.Security;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json.Converters;

using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;

namespace Example.Api.Base;

public static class ApiExtensions
{
    public static async Task RunMigrations<TContext>(this WebApplication app) where TContext : DbContext
    {
        await using var scope = app.Services.CreateAsyncScope();
        await using var authDb = scope.ServiceProvider.GetRequiredService<TContext>();

        await authDb.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
    }

    public static void ConfigureDb<TContext>(this WebApplicationBuilder builder, string serverName, string dbName, bool useInterceptors = false)
        where TContext : DbContext
    {
        var connectionString = builder.Configuration.GetConnectionString(dbName);

        builder.AddNpgsqlDbContext<TContext>(
            serverName,
            x => x.ConnectionString = connectionString,
            x =>
            {
                if (useInterceptors)
                {
                    x.AddInterceptors(new SoftDeleteInterceptor(), new AddOrModifyInterceptor(), new ConcurrentInterceptor());
                }

                x.UseNpgsql();
                x.UseExceptionProcessor();
            });
    }

    public static void ConfigureSwagger(this IServiceCollection serviceeCollection, params IEnumerable<(Type, JsonObjectType)> typesToMap)
    {
        serviceeCollection.SwaggerDocument(options =>
        {
            options.DocumentSettings = settings =>
            {
                foreach (var (type, enumValue) in typesToMap)
                {
                    var objectType = new ObjectTypeMapper(type, new JsonSchema
                    {
                        Type = enumValue,
                    });

                    settings.SchemaSettings.TypeMappers.Add(objectType);
                }

                foreach (var item in typeof(BaseEntity).Assembly.ExportedTypes.Where(p => p.IsEnum))
                {
                    settings.SchemaSettings.TypeMappers.Add(new PrimitiveTypeMapper(item, schema =>
                    {
                        schema.Type = JsonObjectType.String;
                        schema.Format = "string";

                        foreach (var value in Enum.GetValues(item))
                        {
                            schema.Enumeration.Add(value.ToString());
                        }
                    }));
                }

                settings.Title = "API Docs";
                settings.Version = "v1";
            };
            options.ShortSchemaNames = true;
            options.NewtonsoftSettings = static settings => settings.Converters.Add(new StringEnumConverter());
        });
    }

    public static void ConfigureJWT(this WebApplicationBuilder builder)
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

    public static void ConfigureLocalizer(this WebApplicationBuilder builder)
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
}
