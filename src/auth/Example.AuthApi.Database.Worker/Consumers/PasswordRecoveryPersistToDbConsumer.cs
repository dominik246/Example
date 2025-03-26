using Example.AuthApi.Database.Models;
using Example.MassTransit.PasswordRecovery;
using Example.MassTransit.PasswordRecovery.EventModels;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Example.AuthApi.Database.Worker.Consumers;

public sealed class PasswordRecoveryPersistToDbConsumer(AuthDbContext db) : IConsumer<PasswordRecoveryPersist>
{
    public async Task Consume(ConsumeContext<PasswordRecoveryPersist> context)
    {
        bool result = false;
        Exception? exception = null;
        try
        {
            result = await PersistToDb(db, context.Message.UserId, context.Message.Hash, context.CancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (!result)
        {
            await context.Publish(new PasswordRecoveryPersistToDbFailed { Id = context.Message.UserId, Reason = exception?.Message ?? "Failed to persist to Db" }, context.CancellationToken);
            return;
        }

        await context.Publish(new PasswordRecoveryPersistToDbCompleted() { UserId = context.Message.UserId }, context.CancellationToken);
    }

    private static async Task<bool> PersistToDb(AuthDbContext dbContext, Guid userId, string hash, CancellationToken ct)
    {
        var dbResult = await dbContext.UserPasswordRestores.Where(p => p.UserId == userId).ToListAsync(ct);

        if (dbResult is { Count: > 0 })
        {
            dbContext.UserPasswordRestores.RemoveRange(dbResult);
        }

        dbContext.UserPasswordRestores.Add(new UserPasswordRestore { UserId = userId, SecurityCode = hash });

        return dbResult.Count + 1 == await dbContext.SaveChangesAsync(ct);
    }
}
