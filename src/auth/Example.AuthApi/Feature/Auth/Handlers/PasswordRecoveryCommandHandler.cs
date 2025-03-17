using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;
using Example.ServiceDefaults.Consts;
using Example.ServiceDefaults.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using NATS.Client.JetStream;

using StackExchange.Redis;

using System.Security.Cryptography;

namespace Example.AuthApi.Feature.Auth.Handlers;

public sealed record PasswordRecoveryCommand(string Email) : ICommand<bool>;

public sealed class PasswordRecoveryCommandHandler(
    AuthDbContext dbContext, IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig, IStringLocalizer localizer, INatsJSContext natsConnection)
    : CommandHandler<PasswordRecoveryCommand, bool>
{
    public override async Task<bool> ExecuteAsync(PasswordRecoveryCommand command, CancellationToken ct = default)
    {
        if (await CheckIfUserExists(dbContext, command, ct) is not User user)
        {
            AddError(x => x.Email, "User does not exist.", "404");
            return false;
        }

        var hash = await GenerateAndPersistKey(dbContext, cache, cacheConfig, localizer, natsConnection, user, ct);

        if (hash is null)
        {
            AddError(x => x.Email, "User does not exist.", "403");
            return false;
        }

        return true;
    }

    private static async Task<string?> GenerateAndPersistKey(
        AuthDbContext dbContext, IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig, IStringLocalizer localizer, INatsJSContext nats, User user, CancellationToken ct)
    {
        var hash = RandomNumberGenerator.GetHexString(AuthConsts.PasswordRecoveryHexLength);

        await PersistToDb(dbContext, user, hash, ct);
        await PersistToCache(cache, cacheConfig, user, hash);

        var values = new Dictionary<string, string> { { "key", hash } };
        var data = new MailAddressModel(
            localizer[LocalizerConsts.PasswordRecoveryCommandHandler.EmailSubject],
            localizer[LocalizerConsts.PasswordRecoveryCommandHandler.EmailTemplate],
            user.Email,
            values);

        await SendEmail(nats, data, ct);
        return hash;
    }

    private static async Task<bool> SendEmail(INatsJSContext jetStream, MailAddressModel model, CancellationToken ct)
    {
        var response = await jetStream.PublishAsync(NatsEvents.EmailPasswordRecoverEvent, model, MailAddressModelSerializer.Default, cancellationToken: ct);

        return response.IsSuccess();
    }

    private static async Task PersistToCache(IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig, User user, string hash)
    {
        var cacheDb = cache.GetDatabase();
        var key = $"{nameof(UserPasswordRestore)}_{user.Email}";

        await cacheDb.StringSetAsync(key, hash, cacheConfig.Value.ExpiryConfiguration.UserPasswordRestoreExpiry);
    }

    private static async Task<int> PersistToDb(AuthDbContext dbContext, User user, string hash, CancellationToken ct)
    {
        var dbResult = await dbContext.UserPasswordRestores.Where(p => p.UserId == user.Id).ToListAsync(ct);

        if (dbResult is { Count: > 0 })
        {
            dbContext.UserPasswordRestores.RemoveRange(dbResult);
        }

        dbContext.UserPasswordRestores.Add(new UserPasswordRestore { UserId = user.Id, SecurityCode = hash });

        return await dbContext.SaveChangesAsync(ct);
    }

    private static async Task<User?> CheckIfUserExists(AuthDbContext dbContext, PasswordRecoveryCommand command, CancellationToken ct)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email, ct);
    }
}