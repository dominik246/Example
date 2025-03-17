using Example.AuthApi.Database;
using Example.AuthApi.Feature.Auth.Dtos;
using Example.ServiceDefaults.Defaults;

using Microsoft.EntityFrameworkCore;

using StackExchange.Redis;

using System.Text.Json;

namespace Example.AuthApi.Feature.Auth.Handlers;

public sealed record ConfirmEmailCommand(string Hash, string Email) : ICommand<bool>;

public sealed class ConfirmEmailHandler(AuthDbContext db, IConnectionMultiplexer cache) : CommandHandler<ConfirmEmailCommand, bool>
{
    public override async Task<bool> ExecuteAsync(ConfirmEmailCommand command, CancellationToken ct = default)
    {
        var cacheDb = cache.GetDatabase();
        var cacheKey = $"{nameof(ConfirmEmailRequest)}_{command.Email}";

        var cacheHit = await cacheDb.StringGetAsync(cacheKey);

        if (cacheHit is { IsNullOrEmpty: false }
            && JsonSerializer.Deserialize<ConfirmEmailRequest>(cacheHit!, JsonSerializerDefaultValues.CacheOptions) is ConfirmEmailRequest parsed
            && parsed.Hash == command.Hash
            && parsed.Email == command.Email)
        {
            return await ConfirmEmail(db, command.Email, ct) && await InvalidateCache(cacheDb, cacheKey);
        }

        var dbResult = await db.UserEmailConfirms.FirstOrDefaultAsync(p => p.User!.Email == command.Email && p.Hash == command.Hash, ct);

        if (dbResult is null)
        {
            AddError("User is already verified", "404");
            return false;
        }

        return await ConfirmEmail(db, command.Email, ct) && await InvalidateCache(cacheDb, cacheKey);
    }

    private static async Task<bool> ConfirmEmail(AuthDbContext db, string email, CancellationToken ct)
    {
        var dbResult = await db.Users.FirstOrDefaultAsync(p => p.Email == email, ct);

        if (dbResult is null)
        {
            return false;
        }

        dbResult.EmailConfirmed = true;
        db.Users.Update(dbResult);

        var confirmEmailEntries = await db.UserEmailConfirms.Where(p => p.User!.Email == email).ToListAsync(ct);
        db.UserEmailConfirms.RemoveRange(confirmEmailEntries);

        await db.SaveChangesAsync(ct);

        return true;
    }

    private static async Task<bool> InvalidateCache(IDatabase cacheDb, string cacheKey)
    {
        return RedisValue.Null != await cacheDb.StringGetDeleteAsync(cacheKey);
    }
}