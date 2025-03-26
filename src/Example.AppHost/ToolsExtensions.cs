using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;

using Microsoft.Extensions.Configuration;

using System.Reflection;

namespace Example.AppHost;

public static class ToolsExtensions
{
    public static IResourceBuilder<RabbitMQServerResource> AddRabbitMQ(this IDistributedApplicationBuilder builder)
    {
        var config = builder.Configuration.GetRequiredSection(MqConfiguration.SectionName).Get<MqConfiguration>();
        ArgumentNullException.ThrowIfNull(config);

        var username = builder.AddParameter("mq-username", config.Username, false, true);
        var password = builder.AddParameter("mq-password", config.Password, false, true);

        var mq = builder.AddRabbitMQ(ConnectionStrings.RabbitMq, username, password).WithDataVolume().WithManagementPlugin();

        return mq;
    }

    public static IResourceBuilder<SeqResource> ConfigureSeq(this IDistributedApplicationBuilder builder)
    {
        var configuration = builder.Configuration.GetRequiredSection(SeqConfiguration.SectionName).Get<SeqConfiguration>();
        ArgumentNullException.ThrowIfNull(configuration);

        var seq = builder.AddSeq(ConnectionStrings.Seq)
            .ExcludeFromManifest()
            .WithDataBindMount(configuration.DataLocation)
            .WithEnvironment("ACCEPT_EULA", "Y");

        return seq;
    }

    public static IResourceBuilder<ContainerResource> AddDbGate(this IDistributedApplicationBuilder builder)
    {
        var sqlPad = builder
            .AddContainer("dbgate", "dbgate/dbgate", "alpine")
            .WithHttpEndpoint(3001, 3000)
            .ExcludeFromManifest()
            .WithCustomDataVolume("data", "/root/.dbgate");

        return sqlPad;
    }

    private static readonly Type? VolumeNameGeneratorClassType = Assembly.GetAssembly(typeof(DistributedApplication))?.GetType("Aspire.Hosting.Utils.VolumeNameGenerator");
    private static readonly MethodInfo? CreateVolumeNameMethodInfo = VolumeNameGeneratorClassType?.GetMethod("CreateVolumeName", BindingFlags.Static | BindingFlags.Public);
    public static IResourceBuilder<T> WithCustomDataVolume<T>(this IResourceBuilder<T> builder, string suffix, string destination) where T : ContainerResource
    {
        var methodResult = CreateVolumeNameMethodInfo?.MakeGenericMethod(typeof(T)).Invoke(null, [builder, suffix]) as string;

        static string hash(IResourceBuilder<T> builder) => builder.ApplicationBuilder.Configuration["AppHost:Sha256"]![..10].ToLowerInvariant();

        var result = methodResult ?? $"{builder.ApplicationBuilder.Environment.ApplicationName}-{hash(builder)}-{builder.Resource.Name}-{suffix}";

        builder.WithVolume(result, destination);

        return builder;
    }
}
