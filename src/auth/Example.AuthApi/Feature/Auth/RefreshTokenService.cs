using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.AuthApi.Feature.Auth.Dtos;
using Example.ServiceDefaults.Configuration;
using Example.ServiceDefaults.Defaults;

using FastEndpoints.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using StackExchange.Redis;

using System.Linq.Expressions;
using System.Text.Json;

namespace Example.AuthApi.Feature.Auth;

public sealed class RefreshTokenService : RefreshTokenService<CustomTokenRequest, TokenResponse>
{
    private readonly AuthDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly IConnectionMultiplexer _cache;
    private readonly IOptions<AuthCacheConfiguration> _cacheConfig;

    public RefreshTokenService(
        AuthDbContext dbContext,
        TimeProvider timeProvider,
        IOptions<JwtConfiguration> jwtConfig,
        IOptions<AuthCacheConfiguration> cacheConfig,
        IConnectionMultiplexer cache)
    {
        Setup(options =>
        {
            options.TokenSigningKey = jwtConfig.Value.PrivateKey;
            options.TokenSigningStyle = TokenSigningStyle.Asymmetric;
            options.TokenSigningAlgorithm = SecurityAlgorithms.RsaSha256;
            options.SigningKeyIsPemEncoded = true;
            options.Issuer = jwtConfig.Value.Issuer;
            options.Audience = jwtConfig.Value.Audience;
            options.AccessTokenValidity = jwtConfig.Value.AccessTokenValidity;
            options.RefreshTokenValidity = jwtConfig.Value.RefreshTokenValidity;

            options.Endpoint("account/refresh-token", _ => { });
        });

        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _cache = cache;
        _cacheConfig = cacheConfig;
    }

    private static string GetRedisKey(string userId) => $"{nameof(UserToken)}_{userId}";

    public override async Task PersistTokenAsync(TokenResponse response)
    {
        var userToken = new UserToken()
        {
            UserId = Guid.Parse(response.UserId),
            RefreshToken = response.RefreshToken,
            AccessToken = response.AccessToken,
            AccessExpiry = response.AccessExpiry,
            RefreshExpiry = response.RefreshExpiry
        };

        var serialized = JsonSerializer.Serialize(userToken, JsonSerializerDefaultValues.CacheOptions);
        await _cache.GetDatabase().StringSetAsync(GetRedisKey(response.UserId), serialized, _cacheConfig.Value.ExpiryConfiguration.UserTokenExpiry);
    }

    private static readonly Func<RedisValue, CustomTokenRequest, DateTimeOffset, bool> CheckIfCacheTokenIsValid =
        (cacheHit, request, utcNow) =>
            cacheHit is { IsNullOrEmpty: false } && JsonSerializer.Deserialize<UserToken>(cacheHit!, JsonSerializerDefaultValues.CacheOptions) is UserToken cache
            && cache.UserId == Guid.Parse(request.UserId)
            && cache.RefreshToken == request.RefreshToken
            && cache.AccessToken == request.AccessToken
            && cache.RefreshExpiry > utcNow;
    public override async Task RefreshRequestValidationAsync(CustomTokenRequest req)
    {
        var utcNow = _timeProvider.GetUtcNow();

        var cacheHit = await _cache.GetDatabase().StringGetAsync(GetRedisKey(req.UserId));

        if (CheckIfCacheTokenIsValid(cacheHit, req, utcNow))
        {
            return;
        }

        Expression<Func<UserToken, bool>> checkIfDbTokenIsValid =
            p => p.UserId == Guid.Parse(req.UserId) && p.RefreshToken == req.RefreshToken && p.AccessToken == req.AccessToken && p.RefreshExpiry > utcNow;
        var dbResult = await _dbContext.UserTokens.AnyAsync(checkIfDbTokenIsValid);

        if (!dbResult)
        {
            AddError(r => r.RefreshToken, "Refresh token is invalid!");
        }
    }

    public override Task SetRenewalPrivilegesAsync(CustomTokenRequest request, UserPrivileges privileges)
    {
        return Task.CompletedTask;
    }
}
