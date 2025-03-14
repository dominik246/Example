using Example.AuthApi.Feature.Group.Dtos;
using Example.AuthApi.Feature.Group.Handlers;

namespace Example.AuthApi.Feature.Group.Endpoints;

public sealed class UpdateEndpoint : Endpoint<UpdateRequest>
{
    public override void Configure()
    {
        Put("group");
    }

    public override async Task HandleAsync(UpdateRequest req, CancellationToken ct)
    {
        var result = await new UpdateCommand(req.Id, req.Name, req.UserIds).ExecuteAsync(ct);

        if (!result)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
