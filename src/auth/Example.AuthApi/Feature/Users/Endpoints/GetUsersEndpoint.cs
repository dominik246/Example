using Example.AuthApi.Feature.Users.Dtos;
using Example.AuthApi.Feature.Users.Handlers;

using Microsoft.EntityFrameworkCore;

namespace Example.AuthApi.Feature.Users.Endpoints;

public sealed class GetUsersEndpoint : Endpoint<GetUsersRequest, GetUsersResponse>
{
    public override void Configure()
    {
        Get("users");
    }

    public override async Task HandleAsync(GetUsersRequest req, CancellationToken ct)
    {
        var result = await new GetUsersCommand(req.SearchString, req.OnlyEnabled, req.OnlyDisabled).ExecuteAsync(ct);

        await SendOkAsync(result, ct);
    }
}
