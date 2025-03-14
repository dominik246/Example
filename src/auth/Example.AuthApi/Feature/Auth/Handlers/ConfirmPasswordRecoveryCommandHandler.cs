using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using StackExchange.Redis;

using System.Text.Json;

using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.ServiceDefaults.Defaults;

namespace Example.AuthApi.Feature.Auth.Handlers;

public sealed record ConfirmPasswordRecoveryCommand(string Email, string SecurityCode, string NewPassword) : ICommand<bool>;

public sealed class ConfirmPasswordRecoveryCommandHandler(AuthDbContext dbContext, IConnectionMultiplexer cache) : CommandHandler<ConfirmPasswordRecoveryCommand, bool>
{
    public override async Task<bool> ExecuteAsync(ConfirmPasswordRecoveryCommand command, CancellationToken ct = default)
    {
        if (await CheckIfUserExists(dbContext, command, ct) is not User user)
        {
            AddError(x => x.Email, "User does not exist.", "404");
            return false;
        }

        if (await CheckIfUserRequestedToken(dbContext, cache, command, ct) is not UserPasswordRestore userPasswordRestoreEntity)
        {
            AddError(x => x.Email, "User did not request token.", "404");
            return false;
        }

        userPasswordRestoreEntity.User = user;
        var result = await GenerateNewPassword(dbContext, command, userPasswordRestoreEntity, ct);

        if (!result)
        {
            AddError(x => x.Email, "User did not request token.", "404");
            return false;
        }

        return true;
    }

    private static async Task<bool> GenerateNewPassword(AuthDbContext dbContext, ConfirmPasswordRecoveryCommand command, UserPasswordRestore userPasswordRestoreEntity, CancellationToken ct)
    {
        userPasswordRestoreEntity.User!.PasswordHash = new PasswordHasher<User>().HashPassword(userPasswordRestoreEntity.User, command.NewPassword);
        dbContext.UserPasswordRestores.Remove(userPasswordRestoreEntity);

        var availableUserToken = await dbContext.UserTokens.Where(p => p.User!.Email == command.Email).ToListAsync(ct);
        if (availableUserToken is { Count: > 0 })
        {
            dbContext.UserTokens.RemoveRange(availableUserToken);
        }

        dbContext.Users.Update(userPasswordRestoreEntity.User);
        var result = await dbContext.SaveChangesAsync(ct);

        return result is not 0;
    }

    private static async Task<UserPasswordRestore?> CheckIfUserRequestedToken(
        AuthDbContext dbContext, IConnectionMultiplexer cache, ConfirmPasswordRecoveryCommand command, CancellationToken token)
    {
        var cacheDb = cache.GetDatabase();
        var key = $"{nameof(UserPasswordRestore)}_{command.Email}";

        var cacheHit = await cacheDb.StringGetAsync(key);

        if (cacheHit is { IsNullOrEmpty: true }
            && JsonSerializer.Deserialize<UserPasswordRestore>(cacheHit!, JsonSerializerDefaultValues.CacheOptions) is UserPasswordRestore userPasswordRestore
            && userPasswordRestore.SecurityCode == command.SecurityCode)
        {
            return userPasswordRestore;
        }

        return await dbContext.UserPasswordRestores.FirstOrDefaultAsync(x => x.User!.Email == command.Email && x.SecurityCode == command.SecurityCode, token);
    }

    private static Task<User?> CheckIfUserExists(AuthDbContext dbContext, ConfirmPasswordRecoveryCommand command, CancellationToken token)
    {
        return dbContext.Users.FirstOrDefaultAsync(p => p.Email == command.Email, token);
    }
}