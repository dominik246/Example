using Example.ServiceDefaults.Configuration;

using Microsoft.Extensions.Configuration;

namespace Example.AppHost;

public static class DatabaseServersExtensions
{
    public static (IResourceBuilder<PostgresServerResource> Server, IResourceBuilder<PostgresDatabaseResource> Db) ConfigurePostgresServer(
        this IDistributedApplicationBuilder builder, string serverName, string dbName, string configurationSectionName)
    {
        var config = builder.Configuration.GetRequiredSection(configurationSectionName).Get<DatabaseConfiguration>();
        ArgumentNullException.ThrowIfNull(config?.Username);
        ArgumentNullException.ThrowIfNull(config?.Password);

        var username = builder.AddParameter($"{serverName}-{dbName}-Username", config.Username, false, true);
        var password = builder.AddParameter($"{serverName}-{dbName}-Password", config.Password, false, true);

        return builder.ConfigurePostgresServer(username, password, serverName, dbName);
    }

    private static (IResourceBuilder<PostgresServerResource> Server, IResourceBuilder<PostgresDatabaseResource> Db) ConfigurePostgresServer(
        this IDistributedApplicationBuilder builder, IResourceBuilder<ParameterResource> username, IResourceBuilder<ParameterResource> password, string serverName, string dbName)
    {
        var server = builder.AddPostgres(serverName, username, password)
            .WithDataVolume()
            .WithEnvironment("POSTGRES_USER", username)
            .WithEnvironment("POSTGRES_PASSWORD", password)
            .WithEnvironment("POSTGRES_DB", dbName);
        var db = server.AddDatabase(dbName);

        return (server, db);
    }
}
