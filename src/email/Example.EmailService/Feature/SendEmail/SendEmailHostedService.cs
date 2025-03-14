using Microsoft.Extensions.Options;

using NATS.Client.Core;
using NATS.Client.JetStream.Models;
using NATS.Net;

using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;
using Example.ServiceDefaults.Models;

namespace Example.EmailService.Feature.SendEmail;

public sealed class SendEmailHostedService(INatsConnection natsConnection, SendEmailClient emailClient, IOptions<EmailConfiguration> emailConfig) : BackgroundService
{
    private static readonly StreamConfig StreamConfig = new(NatsStreams.EmailStream, [NatsEvents.EmailPasswordRecoverEvent])
    {
    };

    private static readonly ConsumerConfig ConsumerConfig = new(NatsConsumers.EmailConsumer)
    {
    };

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var jetStream = natsConnection.CreateJetStreamContext();

        // this is not the best solution in the world but it works... the issue is that the stream has to be created somewhere;
        // usually it gets created by the dedicated service.
        // since we self-host for dev purposes we don't have such a luxury therefore we have to create it somewhere in the code;
        // another solution would be to spin up one more microservice just for the service, but one microservice for one stream
        // is imo a waste so this will do it for now until we have more streams that require orchestration for spin up :)
        try
        {
            _ = await jetStream.GetStreamAsync(NatsStreams.EmailStream, null, cancellationToken);
        }
        catch (Exception)
        {
            await jetStream.CreateStreamAsync(StreamConfig, cancellationToken);
        }

        await jetStream.CreateConsumerAsync(NatsStreams.EmailStream, ConsumerConfig, cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        var jetStream = natsConnection.CreateJetStreamContext();
        await jetStream.DeleteConsumerAsync(NatsStreams.EmailStream, NatsConsumers.EmailConsumer, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jetStream = natsConnection.CreateJetStreamContext();
        var consumer = await jetStream.GetConsumerAsync(NatsStreams.EmailStream, NatsConsumers.EmailConsumer, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await foreach (var item in consumer.ConsumeAsync(new MailAddressModelSerializer(), cancellationToken: stoppingToken))
            {
                if (item.Data is null)
                {
                    await item.NakAsync(delay: TimeSpan.FromSeconds(20), cancellationToken: stoppingToken);
                    continue;
                }

                var result = await emailClient.SendAsync(item.Data, stoppingToken);

                if (!result)
                {
                    await item.NakAsync(delay: TimeSpan.FromSeconds(20), cancellationToken: stoppingToken);
                    continue;
                }

                await item.AckAsync(cancellationToken: stoppingToken);
            }

            await Task.Delay(emailConfig.Value.SendEmailCooldown, stoppingToken);
        }
    }
}
