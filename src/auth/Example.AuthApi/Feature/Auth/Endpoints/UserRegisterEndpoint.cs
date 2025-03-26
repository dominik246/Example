using Example.AuthApi.Feature.Auth.Dtos;
using Example.AuthApi.Feature.Auth.Handlers;

namespace Example.AuthApi.Feature.Auth.Endpoints;

public sealed class UserRegisterEndpoint : Endpoint<RegisterRequest>
{
    public override void Configure()
    {
        Post("account/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        await new UserRegisterCommand(req.Email, req.Password).ExecuteAsync(ct);
        await SendNoContentAsync(ct);
    }
}