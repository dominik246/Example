using Example.AuthApi.Feature.Auth.Dtos;
using Example.AuthApi.Feature.Auth.Handlers;

namespace Example.AuthApi.Feature.Auth.Endpoints;

public sealed class ConfirmEmailEndpoint : Endpoint<ConfirmEmailRequest>
{
    public override void Configure()
    {
        Get("account/confirm-email");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ConfirmEmailRequest req, CancellationToken ct)
    {
        var commandResult = await new ConfirmEmailCommand(req.Hash, req.Email).ExecuteAsync(ct);

        if (!commandResult)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
