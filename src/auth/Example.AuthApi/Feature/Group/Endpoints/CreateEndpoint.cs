using Example.AuthApi.Feature.Group.Dtos;
using Example.AuthApi.Feature.Group.Handlers;

using Microsoft.EntityFrameworkCore;

namespace Example.AuthApi.Feature.Group.Endpoints;

public sealed class CreateEndpoint : Endpoint<CreateGroupRequest>
{
    public override void Configure()
    {
        Post("group/create");
    }

    public override async Task HandleAsync(CreateGroupRequest req, CancellationToken ct)
    {
        var result = await new CreateGroupCommand(req.Name, req.IsAdminGroup, req.ShouldAutoAddSelf, req.UserIds).ExecuteAsync(ct);

        if (!result)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
