using Example.ServiceDefaults.Configuration;
using Example.ServiceDefaults.Consts;

using FastEndpoints.Security;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Example.NotificationsApi.Feature.Auth;

public sealed class TokenRevocationMiddleware(RequestDelegate next, IServiceProvider serviceProvider) : JwtRevocationMiddleware(next)
{
    protected override async Task<bool> JwtTokenIsValidAsync(string jwtToken, CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var (utcNow, httpContextAccessor, config) = GetFromServiceProvider(scope.ServiceProvider);

        if (httpContextAccessor.HttpContext is null)
        {
            return false;
        }

        var readResult = AssignJwtContextToHttpContext(jwtToken, httpContextAccessor.HttpContext);

        if (readResult is null)
        {
            return false;
        }

        var userId = httpContextAccessor.HttpContext?.User.ClaimValue(JwtClaimConsts.UserId);

        // cheap check so better do it now than in CheckIfAccessTokenIsValid
        return !string.IsNullOrWhiteSpace(userId)
            && readResult.ValidFrom < utcNow
            && readResult.ValidTo > utcNow
            && httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated is true
            && readResult.Issuer == config.Value.Issuer
            && readResult.Audiences.Contains(config.Value.Audience);
    }

    private static JwtSecurityToken? AssignJwtContextToHttpContext(string jwtToken, HttpContext httpContext)
    {
        var handler = new JwtSecurityTokenHandler();

        JwtSecurityToken? readResult;
        try
        {
            readResult = handler.ReadJwtToken(jwtToken);
        }
        catch (Exception)
        {
            return null;
        }

        if (httpContext.User is not { Identity.IsAuthenticated: true })
        {
            var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.NameIdentifier, ClaimTypes.Role);
            claimsIdentity.AddClaims(readResult.Claims);
            httpContext.User = new ClaimsPrincipal(claimsIdentity);
        }

        return readResult;
    }

    internal static (DateTimeOffset, IHttpContextAccessor, IOptions<JwtConfiguration>) GetFromServiceProvider(IServiceProvider serviceProvider)
    {
        var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var utcNowTime = timeProvider.GetUtcNow();
        var config = serviceProvider.GetRequiredService<IOptions<JwtConfiguration>>();

        return (utcNowTime, httpContextAccessor, config);
    }
}
