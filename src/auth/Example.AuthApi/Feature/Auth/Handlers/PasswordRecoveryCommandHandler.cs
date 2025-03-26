using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.AuthApi.Localization;
using Example.MassTransit.PasswordRecovery.EventModels;
using Example.ServiceDefaults.Configuration;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

using System.Security.Cryptography;

namespace Example.AuthApi.Feature.Auth.Handlers;

public sealed record PasswordRecoveryCommand(string Email) : ICommand;

public sealed class PasswordRecoveryCommandHandler(AuthDbContext dbContext, AuthOutboxDbContext outboxDbContext, IStringLocalizer localizer, IPublishEndpoint publishEndpoint)
    : CommandHandler<PasswordRecoveryCommand>
{
    public override async Task ExecuteAsync(PasswordRecoveryCommand command, CancellationToken ct = default)
    {
        if (await CheckIfUserExists(dbContext, command, ct) is not User user)
        {
            ThrowError(x => x.Email, "User does not exist.", 404);
            return;
        }

        var hash = RandomNumberGenerator.GetHexString(AuthConsts.PasswordRecoveryHexLength);
        var model = new PasswordRecoveryModel
        {
            Email = user.Email,
            EmailSubject = localizer[LocalizerConsts.PasswordRecoveryCommandHandler.EmailSubject],
            EmailTemplate = localizer[LocalizerConsts.PasswordRecoveryCommandHandler.EmailTemplate],
            Hash = hash,
            UserId = user.Id
        };

        await publishEndpoint.Publish(model, ct);
        await outboxDbContext.SaveChangesAsync(ct);
    }

    private static async Task<User?> CheckIfUserExists(AuthDbContext dbContext, PasswordRecoveryCommand command, CancellationToken ct)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email, ct);
    }
}

public sealed class PersistToCacheConsumer(IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig) : IConsumer<PasswordRecoveryPersist>
{
    public async Task Consume(ConsumeContext<PasswordRecoveryPersist> context)
    {
        var result = await PersistToCache(cache, cacheConfig, context.Message.Email, context.Message.Hash);

        if (!result)
        {
            await context.Publish(new PasswordRecoveryPersistToDbFailed { Id = context.Message.UserId, Reason = "Cache persistance failed." }, context.CancellationToken);
        }
    }

    private static async Task<bool> PersistToCache(IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig, string email, string hash)
    {
        var cacheDb = cache.GetDatabase();
        var key = $"{nameof(UserPasswordRestore)}_{email}";

        return await cacheDb.StringSetAsync(key, hash, cacheConfig.Value.ExpiryConfiguration.UserPasswordRestoreExpiry);
    }
}