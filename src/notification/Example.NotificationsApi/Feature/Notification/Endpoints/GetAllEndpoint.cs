using Example.NotificationsApi.Feature.Notification.Dtos;
using Example.NotificationsApi.Feature.Notification.Handlers;

using Microsoft.EntityFrameworkCore;

namespace Example.NotificationsApi.Feature.Notification.Endpoints;

public sealed class GetAllEndpoint : Endpoint<GetAllRequest>
{
    public override void Configure()
    {
        Get("notification");
    }

    public override async Task HandleAsync(GetAllRequest req, CancellationToken ct)
    {
        var result = await new GetAllCommand(req.Status, req.StartDate, req.EndDate).ExecuteAsync(ct);

        await SendOkAsync(result, ct);
    }
}
