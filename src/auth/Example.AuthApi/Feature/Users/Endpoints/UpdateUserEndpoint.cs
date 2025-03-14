using Example.AuthApi.Feature.Users.Dtos;
using Example.AuthApi.Feature.Users.Handlers;

namespace Example.AuthApi.Feature.Users.Endpoints;

public sealed class UpdateUserEndpoint : Endpoint<UpdateUserRequest>
{
    public override void Configure()
    {
        Put("users");
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var result = await new UpdateUserCommand(req.Id, req.IsDisabled, req.IsEmailConfirmed).ExecuteAsync(ct);

        if (!result)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
