using Example.MassTransit;
using Example.MassTransit.SendEmail;

using MassTransit;

namespace Example.EmailService.Feature.SendEmail;

public sealed class SendEmailConsumer<TSuccess, TFailure, TData>(SendEmailClient emailClient) : IConsumer<TData>
    where TSuccess : SendEmailCompleted, new()
    where TFailure : FailedEvent, new()
    where TData : AbstractMailAddressModel
{
    public async Task Consume(ConsumeContext<TData> context)
    {
        bool result = false;
        Exception? exception = null;
        try
        {
            result = await emailClient.SendAsync(context.Message, context.CancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (!result)
        {
            await context.Publish(new TFailure { Id = context.Message.Id, Reason = exception?.Message ?? "Email failed to be sent." }, context.CancellationToken);
            return;
        }

        await context.Publish(new TSuccess { Id = context.Message.Id }, context.CancellationToken);
    }
}
