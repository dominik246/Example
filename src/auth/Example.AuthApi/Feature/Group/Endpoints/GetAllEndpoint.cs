using Example.AuthApi.Feature.Group.Dtos;
using Example.AuthApi.Feature.Group.Handlers;

namespace Example.AuthApi.Feature.Group.Endpoints;

public sealed class GetAllEndpoint : Endpoint<GetAllEndpointRequest, GetAllResponse>
{
    public override void Configure()
    {
        Get("group");
    }

    public override async Task HandleAsync(GetAllEndpointRequest req, CancellationToken ct)
    {
        var result = await new GetAllEndpointCommand(req.SearchString, req.OnlySelf, req.OnlyOwned).ExecuteAsync(ct);
        await SendOkAsync(result, ct);
    }
}
