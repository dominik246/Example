using Example.Api.Base.Consts;
using Example.AuthApi.Feature.Auth.Dtos;
using Example.AuthApi.Feature.Auth.Handlers;

using FastEndpoints.Security;

using System.Security.Claims;
using System.Text.Json;

namespace Example.AuthApi.Feature.Auth.Endpoints;

public sealed class UserLoginEndpoint : Endpoint<LoginRequest, TokenResponse>
{
    public override void Configure()
    {
        Post("account/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var commandResult = await new UserLoginCommand(req.Email, req.Password).ExecuteAsync(ct);

        if (commandResult is null)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        var nameIdentifiter = new Claim(ClaimTypes.NameIdentifier, req.Email);
        var groups = new Claim(JwtClaimConsts.Groups, JsonSerializer.Serialize(commandResult.UserGroups!.Select(x => x.GroupId)));
        var clientId = new Claim(JwtClaimConsts.UserId, commandResult.Id.ToString());

        var tokenResult = await CreateTokenWith<RefreshTokenService>(
            clientId.Value, x => x.Claims.Add(nameIdentifiter, groups, clientId), req);

        await SendOkAsync(tokenResult, ct);
    }
}