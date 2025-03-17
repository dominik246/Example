
using Example.AuthApi.Feature.Group.Dtos;
using Example.AuthApi.Feature.Group.Handlers;

using Microsoft.EntityFrameworkCore;

namespace Example.AuthApi.Feature.Group.Endpoints;

public sealed class GetByIdEndpoint : Endpoint<GetByIdRequest, GetByIdResponse>
{
    public override void Configure()
    {
        Get("group/{id:guid}");
    }

    public override async Task HandleAsync(GetByIdRequest req, CancellationToken ct)
    {
        var result = await new GetByIdCommand(req.Id).ExecuteAsync(ct);

        if (result is null)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendOkAsync(result, ct);
    }
}
