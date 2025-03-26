using Example.Api.Base;
using Example.Api.Base.Consts;
using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.ServiceDefaults.Configuration;

using FastEndpoints.Security;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Example.AuthApi.Feature.Auth;

public sealed class TokenRevocationMiddleware(RequestDelegate next, IServiceProvider serviceProvider) : JwtRevocationMiddleware(next)
{
    protected override async Task<bool> JwtTokenIsValidAsync(string jwtToken, CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var (dbContext, utcNow, httpContextAccessor, cache, config) = GetFromServiceProvider(scope.ServiceProvider);

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
        if (string.IsNullOrWhiteSpace(userId)
            || readResult.ValidFrom >= utcNow
            || readResult.ValidTo <= utcNow
            || httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated is not true
            || readResult.Issuer != config.Value.Issuer
            || !readResult.Audiences.Contains(config.Value.Audience))
        {
            return false;
        }

        return await CheckIfAccessTokenIsValid(dbContext, cache, utcNow, userId, jwtToken, ct);
    }

    private static string GetRedisKey(string userId) => $"{nameof(UserToken)}_{userId}";

    private static async Task<bool> CheckIfAccessTokenIsValid(AuthDbContext dbContext, IConnectionMultiplexer cache, DateTimeOffset utcNow, string userId, string jwtToken, CancellationToken ct)
    {
        var redisKey = GetRedisKey(userId);
        var redisDb = cache.GetDatabase();
        var cacheHit = await redisDb.StringGetAsync(redisKey);

        if (cacheHit is not { IsNullOrEmpty: false })
        {
            return await dbContext.UserTokens.AnyAsync(x => x.UserId == Guid.Parse(userId) && x.RefreshExpiry > utcNow && x.AccessExpiry > utcNow, ct);
        }

        if (JsonSerializer.Deserialize<UserToken>(cacheHit!, JsonSerializerDefaultValues.CacheOptions) is not UserToken userToken
            || userToken.AccessExpiry <= utcNow
            || userToken.AccessToken != jwtToken)
        {
            await redisDb.KeyDeleteAsync(redisKey);
            return false;
        }

        return userToken.AccessExpiry > utcNow && userToken.RefreshExpiry > utcNow && userToken.AccessToken == jwtToken;
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

    internal static (AuthDbContext, DateTimeOffset, IHttpContextAccessor, IConnectionMultiplexer, IOptions<JwtConfiguration>) GetFromServiceProvider(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<AuthDbContext>();
        var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var utcNowTime = timeProvider.GetUtcNow();
        var cache = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        var config = serviceProvider.GetRequiredService<IOptions<JwtConfiguration>>();

        return (dbContext, utcNowTime, httpContextAccessor, cache, config);
    }
}
