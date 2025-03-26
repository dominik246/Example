using Example.ServiceDefaults;
using MassTransit;
using MassTransit.Logging;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Reflection;

namespace Example.MassTransit;

public static class MassTransitInitExtensions
{
    public static void AddCustomMassTransit(this IHostApplicationBuilder builder, Action<IBusRegistrationConfigurator> action)
    {
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString(ConnectionStrings.RabbitMq);
        ArgumentException.ThrowIfNullOrWhiteSpace(rabbitMqConnectionString);

        builder.Services.AddMassTransit(options =>
        {
            options.SetKebabCaseEndpointNameFormatter();

            var entryAssembly = Assembly.GetEntryAssembly();
            options.AddSagaStateMachines(entryAssembly);
            options.AddSagas(entryAssembly);
            options.AddActivities(entryAssembly);
            options.AddConsumers(entryAssembly);

            action?.Invoke(options);

            options.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqConnectionString));
                cfg.UseMessageRetry(r => r.Exponential(10, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5)));
                cfg.ConfigureEndpoints(context);
            });
        });

        builder.Services.AddOpenTelemetry()
                .WithMetrics(b => b.AddMeter(DiagnosticHeaders.DefaultListenerName))
                .WithTracing(providerBuilder =>
                {
                    providerBuilder.AddSource(DiagnosticHeaders.DefaultListenerName);
                });
    }

    public static void AddCustomMassTransit<TContext>(this IHostApplicationBuilder builder, bool useOutbox, Action<IBusRegistrationConfigurator> action) where TContext : DbContext
    {
        void newAction(IBusRegistrationConfigurator x)
        {
            action.Invoke(x);

            x.AddEntityFrameworkOutbox<TContext>(options =>
            {
                options.UsePostgres();
                options.DisableInboxCleanupService();
                options.QueryDelay = TimeSpan.FromSeconds(5);

                if (useOutbox)
                {
                    options.UseBusOutbox();
                }
                else
                {
                    options.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                }
            });

            x.AddConfigureEndpointsCallback(static (context, _, cfg) =>
            {
                cfg.UseEntityFrameworkOutbox<TContext>(context);
            });
        }

        AddCustomMassTransit(builder, newAction);
    }
}
