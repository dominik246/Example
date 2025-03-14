using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Net;

using StackExchange.Redis;

using System.Security.Cryptography;
using System.Text.Json;

using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.AuthApi.Feature.Auth.Dtos;
using Example.ServiceDefaults;
using Example.ServiceDefaults.Configuration;
using Example.ServiceDefaults.Consts;
using Example.ServiceDefaults.Defaults;
using Example.ServiceDefaults.Models;

namespace Example.AuthApi.Feature.Auth.Handlers;

public sealed record UserRegisterCommand(string Email, string Password) : ICommand<bool>;

public sealed class UserRegisterCommandHandler(
    AuthDbContext dbContext, IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig, IStringLocalizer localizer, INatsConnection natsConnection)
    : CommandHandler<UserRegisterCommand, bool>
{
    public override async Task<bool> ExecuteAsync(UserRegisterCommand command, CancellationToken ct = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == command.Email, ct);
        if (user is not null)
        {
            AddError(x => x.Email, "User already exists.", "403");
            return false;
        }

        var newUser = CreateNewUser(command);

        var emailConfirmHash = RandomNumberGenerator.GetHexString(AuthConsts.ConfirmEmailHexLength);

        await PersistUserIntoCache(cache, cacheConfig, newUser);
        await PersistEmailConfirmIntoCache(cache, cacheConfig, emailConfirmHash, newUser);
        await PersistIntoDb(dbContext, newUser, ct); // TODO: db writer

        var values = new Dictionary<string, string> { { "key", emailConfirmHash } };
        var data = new MailAddressModel(
            localizer[LocalizerConsts.UserRegisterCommandHandler.EmailSubject],
            localizer[LocalizerConsts.UserRegisterCommandHandler.EmailTemplate],
            newUser.Email,
            values);

        return await SendEmail(natsConnection, data, ct);
    }

    private static User CreateNewUser(UserRegisterCommand command)
    {
        var userEntity = new User { Email = command.Email };
        userEntity.PasswordHash = new PasswordHasher<User>().HashPassword(userEntity, command.Password);

        return userEntity;
    }

    private static async Task PersistIntoDb(AuthDbContext dbContext, User userEntity, CancellationToken ct)
    {
        dbContext.Users.Add(userEntity);

        await dbContext.SaveChangesAsync(ct);
    }

    private static async Task PersistUserIntoCache(IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig, User user)
    {
        var db = cache.GetDatabase();
        var key = $"{nameof(User)}_{user.Email}";

        var cacheValue = JsonSerializer.Serialize(user, JsonSerializerDefaultValues.CacheOptions);
        await db.StringSetAsync(key, cacheValue, cacheConfig.Value.ExpiryConfiguration.UserExpiry);
    }

    private static async Task PersistEmailConfirmIntoCache(IConnectionMultiplexer cache, IOptions<AuthCacheConfiguration> cacheConfig, string emailConfirmHash, User user)
    {
        var db = cache.GetDatabase();
        var key = $"{nameof(ConfirmEmailRequest)}_{user.Email}";

        var value = new ConfirmEmailRequest { Email = user.Email, Hash = emailConfirmHash };

        var cacheValue = JsonSerializer.Serialize(value, JsonSerializerDefaultValues.CacheOptions);
        await db.StringSetAsync(key, cacheValue, cacheConfig.Value.ExpiryConfiguration.EmailConfirmExpiry);
    }

    private static async Task<bool> SendEmail(INatsConnection connection, MailAddressModel model, CancellationToken ct)
    {
        var jetStream = connection.CreateJetStreamContext();
        var response = await jetStream.PublishAsync(NatsEvents.EmailPasswordRecoverEvent, model, new MailAddressModelSerializer(), cancellationToken: ct);

        return response.IsSuccess();
    }
}