using Example.EmailService.Feature.SendEmail;
using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Example.EmailService;

public static class Program
{
    public static async Task Main(params string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Add service defaults & Aspire client integrations.
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddProblemDetails();

        builder.Services.AddAuthorization();

        builder.Services.AddResilienceEnricher();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddHttpContextAccessor();

        builder.AddSeqEndpoint(ConnectionStrings.Seq);
        builder.AddNatsClient(ConnectionStrings.NatsServer);
        builder.AddNatsJetStream();

        builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetRequiredSection(EmailConfiguration.SectionName));

        builder.Services.AddHttpClient<SendEmailClient>().AddStandardResilienceHandler();
        builder.Services.AddHostedService<SendEmailHostedService>();

        var app = builder.Build();

        await app.RunAsync();
    }
}
