using Example.EmailService.Feature.SendEmail;
using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;

namespace Example.EmailService;

public static class Program
{
    public static async Task Main(params string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetRequiredSection(EmailConfiguration.SectionName));

        builder.Services.AddHttpClient<SendEmailClient>().AddStandardResilienceHandler();
        builder.Services.AddHostedService<SendEmailHostedService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseExceptionHandler();

        app.MapDefaultEndpoints();

        await app.RunAsync();
    }
}
