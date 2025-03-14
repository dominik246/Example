using Aspire.Hosting.Redis;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;
using System.Text;

using Example.ServiceDefaults.Configuration;

namespace Example.AppHost;

public static class CacheExtensions
{
    public static IResourceBuilder<RedisResource> ConfigureCache(this IDistributedApplicationBuilder builder, string configSectionName, string resourceName)
    {
        var config = builder.Configuration.GetRequiredSection(configSectionName).Get<CacheConfiguration>();
        ArgumentNullException.ThrowIfNull(config);

        var cache = builder.AddRedis(resourceName)
            .WithDataVolume()
            .WithPersistence(config.PersistanceInterval, config.KeyChangeThreshold);

        return cache;
    }

    private static readonly Type? RedisBuilderExtensionsClassType = typeof(RedisBuilderExtensions);
    private static readonly MethodInfo[]? RedisBuilderExtensionsClassTypeMethods = RedisBuilderExtensionsClassType?.GetMethods(BindingFlags.Static);
    private static readonly MethodInfo? ImportRedisDatabasesMethodInfo = RedisBuilderExtensionsClassTypeMethods?.FirstOrDefault(p => p.Name.Contains("ImportRedisDatabases"));
    public static IResourceBuilder<RedisInsightResource> ConfigureRedisInsight(this IDistributedApplicationBuilder builder)
    {
        var resource = new RedisInsightResource("redis-insight");
        var resourceBuilder = builder.AddResource(resource)
                                  .WithImage("redis/redisinsight", "2.58")
                                  .WithHttpEndpoint(targetPort: 5540, name: "http")
                                  .ExcludeFromManifest();

        // We need to wait for all endpoints to be allocated before attempting to import databases
        var endpointsAllocatedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        builder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>((_, _) =>
        {
            endpointsAllocatedTcs.TrySetResult();
            return Task.CompletedTask;
        });

        builder.Eventing.Subscribe<ResourceReadyEvent>(resource, async (e, ct) =>
        {
            var redisInstances = builder.Resources.OfType<RedisResource>();

            if (!redisInstances.Any())
            {
                // No-op if there are no Redis resources present.
                return;
            }

            // Wait for all endpoints to be allocated before attempting to import databases
            await endpointsAllocatedTcs.Task;

            var redisInsightResource = builder.Resources.OfType<RedisInsightResource>().Single();
            var insightEndpoint = redisInsightResource.PrimaryEndpoint;

            using var client = new HttpClient() { BaseAddress = new Uri($"{insightEndpoint.Scheme}://{insightEndpoint.Host}:{insightEndpoint.Port}") };

            var rls = e.Services.GetRequiredService<ResourceLoggerService>();
            var resourceLogger = rls.GetLogger(resource);

            var importRedisDatabasesTask = ImportRedisDatabasesMethodInfo?.Invoke(null, [resourceLogger, redisInstances, client, ct]) as Task;
            ArgumentNullException.ThrowIfNull(importRedisDatabasesTask);
            await importRedisDatabasesTask;
        });

        return resourceBuilder;
    }

    public static IResourceBuilder<RedisCommanderResource> ConfigureRedisCommander(this IDistributedApplicationBuilder builder)
    {
        var resource = new RedisCommanderResource("redis-commander");
        var resourceBuilder = builder.AddResource(resource)
                                  .WithImage("rediscommander/redis-commander", "latest")
                                  .WithHttpEndpoint(targetPort: 8081, name: "http")
                                  .ExcludeFromManifest();

        builder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>((_, _) =>
        {
            var redisInstances = builder.Resources.OfType<RedisResource>();

            if (!redisInstances.Any())
            {
                // No-op if there are no Redis resources present.
                return Task.CompletedTask;
            }

            var hostsVariableBuilder = new StringBuilder();

            foreach (var redisInstance in redisInstances)
            {
                if (redisInstance.PrimaryEndpoint.IsAllocated)
                {
                    // Redis Commander assumes Redis is being accessed over a default Aspire container network and hardcodes the resource address
                    // This will need to be refactored once updated service discovery APIs are available
                    var hostString = $"{(hostsVariableBuilder.Length > 0 ? "," : string.Empty)}{redisInstance.Name}:{redisInstance.Name}:{redisInstance.PrimaryEndpoint.TargetPort}:0";
                    hostsVariableBuilder.Append(hostString);
                }
            }

            resourceBuilder.WithEnvironment("REDIS_HOSTS", hostsVariableBuilder.ToString());

            return Task.CompletedTask;
        });

        return resourceBuilder;
    }
}
