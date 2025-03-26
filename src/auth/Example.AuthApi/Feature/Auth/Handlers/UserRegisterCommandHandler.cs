using Example.Api.Base;
using Example.Api.Base.Consts;
using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.AuthApi.Feature.Auth.Dtos;
using Example.AuthApi.Localization;
using Example.MassTransit.RegisterNewUser.EventModels;
using Example.ServiceDefaults.Configuration;

using MassTransit;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

using System.Security.Cryptography;
using System.Text.Json;

namespace Example.AuthApi.Feature.Auth.Handlers;

public sealed record UserRegisterCommand(string Email, string Password) : ICommand;

public sealed class UserRegisterCommandHandler(
    AuthDbContext dbContext, IStringLocalizer localizer, IPublishEndpoint publishEndpoint)
    : CommandHandler<UserRegisterCommand>
{
    public override async Task ExecuteAsync(UserRegisterCommand command, CancellationToken ct = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email, ct);
        if (user is not null)
        {
            ThrowError(x => x.Email, "User already exists.", 403);
            return;
        }

        var userId = Guid.CreateVersion7();
        var emailConfirmHash = RandomNumberGenerator.GetHexString(AuthConsts.ConfirmEmailHexLength);

        var publishModel = new RegisterNewUserModel
        {
            Email = command.Email,
            EmailConfirmHash = emailConfirmHash,
            EmailSubject = localizer[LocalizerConsts.UserRegisterCommandHandler.EmailSubject],
            EmailTemplate = localizer[LocalizerConsts.UserRegisterCommandHandler.EmailTemplate],
            UserId = userId,
            PasswordHash = new PasswordHasher<User>().HashPassword(null!, command.Password),
        };

        await publishEndpoint.Publish(publishModel, ct);
    }
}

public sealed class RegisterUserCacheConsumer(IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig) : IConsumer<PersistNewUser>
{
    public async Task Consume(ConsumeContext<PersistNewUser> context)
    {
        var userResult = await PersistUserIntoCache(cache, cacheConfig, context.Message);
        var emailResult = await PersistEmailConfirmIntoCache(cache, cacheConfig, context.Message);

        if (!userResult || !emailResult)
        {
            await context.Publish(new RegisterNewUserPersistToDbFailed { Id = context.Message.UserId, Reason = "Cache failed to persist." }, context.CancellationToken);
            return;
        }
    }

    private static async Task<bool> PersistUserIntoCache(IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig, PersistNewUser context)
    {
        var db = cache.GetDatabase();
        var key = $"{nameof(User)}_{context.Email}";

        var cacheValue = JsonSerializer.Serialize(context, JsonSerializerDefaultValues.CacheOptions);
        return await db.StringSetAsync(key, cacheValue, cacheConfig.Value.ExpiryConfiguration.UserExpiry);
    }

    private static async Task<bool> PersistEmailConfirmIntoCache(IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig, PersistNewUser context)
    {
        var db = cache.GetDatabase();
        var key = $"{nameof(ConfirmEmailRequest)}_{context.Email}";

        var value = new ConfirmEmailRequest { Email = context.Email, Hash = context.EmailConfirmHash };

        var cacheValue = JsonSerializer.Serialize(value, JsonSerializerDefaultValues.CacheOptions);
        return await db.StringSetAsync(key, cacheValue, cacheConfig.Value.ExpiryConfiguration.EmailConfirmExpiry);
    }
}
