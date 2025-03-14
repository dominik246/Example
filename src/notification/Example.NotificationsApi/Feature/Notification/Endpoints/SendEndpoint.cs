using Example.NotificationsApi.Feature.Notification.Dtos;
using Example.NotificationsApi.Feature.Notification.Handlers;

namespace Example.NotificationsApi.Feature.Notification.Endpoints;

public sealed class SendEndpoint : Endpoint<SendRequest>
{
    public override void Configure()
    {
        Post("notification");
    }

    public override async Task HandleAsync(SendRequest req, CancellationToken ct)
    {
        var result = await new SendCommand(req.UserIds, req.GroupIds, req.Message, req.Severity).ExecuteAsync(ct);

        if (!result)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
