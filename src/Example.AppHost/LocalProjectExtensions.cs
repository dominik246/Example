using Microsoft.Extensions.Configuration;

namespace Example.AppHost;

public static class LocalProjectExtensions
{
    public static IResourceBuilder<ProjectResource> WithPostgresDb(
        this IResourceBuilder<ProjectResource> builder, string dbServerName, string dbName, string dbSectionName, out IResourceBuilder<PostgresServerResource> sqlServer, out IResourceBuilder<PostgresDatabaseResource> db)
    {
        (sqlServer, db) = builder.ApplicationBuilder.ConfigurePostgresServer(dbServerName, dbName, dbSectionName);

        return builder.WithReference(db).WaitFor(db).WithRelationship(sqlServer.Resource, "WaitFor");
    }

    public static IResourceBuilder<ProjectResource> WithCache(
        this IResourceBuilder<ProjectResource> builder, string cacheSectionName, string cacheResourceName, out IResourceBuilder<RedisResource> cache)
    {
        cache = builder.ApplicationBuilder.ConfigureCache(cacheSectionName, cacheResourceName);
        var cacheSection = builder.ApplicationBuilder.Configuration.GetRequiredSection(cacheSectionName);

        return builder
            .WithReference(cache)
            .WaitFor(cache)
            .WithEnvironment(context => context.AddEnvVars(cacheSection));
    }

    public static IResourceBuilder<ProjectResource> WithEnvironmentSection(this IResourceBuilder<ProjectResource> builder, params ReadOnlySpan<string> sectionNames)
    {
        foreach (var sectionName in sectionNames)
        {
            var sectionChildren = builder.ApplicationBuilder.Configuration.GetRequiredSection(sectionName);

            builder.WithEnvironment(context => context.AddEnvVars(sectionChildren));
        }

        return builder;
    }

    public static IResourceBuilder<ProjectResource> ConfigureApi<TProject>(this IDistributedApplicationBuilder builder, string apiName)
        where TProject : IProjectMetadata, new()
    {
        return builder.AddProject<TProject>(apiName);
    }

    private static void AddEnvVars(this EnvironmentCallbackContext context, IConfigurationSection section)
    {
        if (section.Value is not null)
        {
            context.EnvironmentVariables.Add(section.Path, section.Value!);
            return;
        }

        foreach (var child in section.GetChildren())
        {
            context.AddEnvVars(child);
        }
    }
}
