using Microsoft.EntityFrameworkCore;

using Example.AuthApi.Feature.Group.Dtos;
using Example.AuthApi.Feature.Group.Handlers;

namespace Example.AuthApi.Feature.Group.Endpoints;

public sealed class DeleteEndpoint : Endpoint<DeleteEndpointRequest>
{
    public override void Configure()
    {
        Delete("group");
    }

    public override async Task HandleAsync(DeleteEndpointRequest req, CancellationToken ct)
    {
        var result = await new DeleteEndpointCommand(req.Id).ExecuteAsync(ct);

        if (!result)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
