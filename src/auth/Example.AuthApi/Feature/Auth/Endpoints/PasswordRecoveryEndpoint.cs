using Example.AuthApi.Feature.Auth.Dtos;
using Example.AuthApi.Feature.Auth.Handlers;

namespace Example.AuthApi.Feature.Auth.Endpoints;

public sealed class PasswordRecoveryEndpoint : Endpoint<PasswordRecoveryRequest, string>
{
    public override void Configure()
    {
        Post("account/reset-password");
        AllowAnonymous();
    }

    public override async Task HandleAsync(PasswordRecoveryRequest req, CancellationToken ct)
    {
        await new PasswordRecoveryCommand(req.Email).ExecuteAsync(ct);
        await SendNoContentAsync(ct);
    }
}