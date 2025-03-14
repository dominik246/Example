using Example.AuthApi.Feature.Auth.Dtos;
using Example.AuthApi.Feature.Auth.Handlers;

namespace Example.AuthApi.Feature.Auth.Endpoints;

public sealed class ConfirmPasswordRecoveryEndpoint : Endpoint<ConfirmPasswordRecoveryRequest>
{
    public override void Configure()
    {
        Post("account/confirm-password-recovery");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ConfirmPasswordRecoveryRequest req, CancellationToken ct)
    {
        var result = await new ConfirmPasswordRecoveryCommand(req.Email, req.SecurityCode, req.NewPassword).ExecuteAsync(ct);

        if (!result)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}