using Example.AuthApi.Database;
using Example.EmailService.Feature.SendEmail;
using Example.MassTransit;
using Example.MassTransit.PasswordRecovery.EventModels;
using Example.MassTransit.RegisterNewUser.EventModels;
using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;
using Example.Worker.Base;

namespace Example.EmailService;

public static class Program
{
    public static async Task Main(params string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.WebHost.UseKestrelHttpsConfiguration();

        // Add service defaults & Aspire client integrations.
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddProblemDetails();

        builder.Services.AddAuthorization();

        builder.Services.AddResilienceEnricher();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddHttpContextAccessor();

        builder.AddSeqEndpoint(ConnectionStrings.Seq);
        builder.ConfigureDb<AuthOutboxDbContext>(ConnectionStrings.AuthOutboxServer, DatabaseNames.AuthOutboxDatabase, false);

        builder.AddCustomMassTransit<AuthOutboxDbContext>(false, static options =>
        {
            options.AddConsumer<SendEmailConsumer<RegisterUserEmailSentCompleted, RegisterNewUserEmailSendFailed, RegisterNewUserMailAddressModel>>();
            options.AddConsumer<SendEmailConsumer<PasswordRecoveryEmailSentCompleted, PasswordRecoveryEmailSentFailed, PasswordRecoveryMailAddressModel>>();
        });

        builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetRequiredSection(EmailConfiguration.SectionName));

        builder.Services.AddHttpClient<SendEmailClient>().AddStandardResilienceHandler();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        await app.RunAsync();
    }
}
