using Example.NotificationsApi.Feature.Notification.Dtos;
using Example.NotificationsApi.Feature.Notification.Handlers;

using Microsoft.EntityFrameworkCore;

namespace Example.NotificationsApi.Feature.Notification.Endpoints;

public sealed class MarkAsReadEndpoint : Endpoint<MarkAsReadRequest>
{
    public override void Configure()
    {
        Post("notification/markasread");
    }

    public override async Task HandleAsync(MarkAsReadRequest req, CancellationToken ct)
    {
        var result = await new MarkAsReadCommand(req.NotificationId).ExecuteAsync(ct);

        if (!result)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
