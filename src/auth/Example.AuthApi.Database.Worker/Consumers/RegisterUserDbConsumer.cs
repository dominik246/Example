using Example.AuthApi.Database.Models;
using Example.MassTransit.RegisterNewUser.EventModels;

using MassTransit;

namespace Example.AuthApi.Database.Worker.Consumers;

public sealed class RegisterUserDbConsumer(AuthDbContext db) : IConsumer<PersistNewUser>
{
    public async Task Consume(ConsumeContext<PersistNewUser> context)
    {
        var newUser = CreateNewUser(context.Message);

        bool result = false;
        Exception? exception = null;

        try
        {
            result = await PersistIntoDb(db, newUser, context.CancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (!result)
        {
            await context.Publish(new RegisterNewUserPersistToDbFailed { Id = context.Message.UserId, Reason = exception?.Message ?? "Persist to Db failed." }, context.CancellationToken);
        }

        await context.Publish(new RegisterUserPersistToDbCompleted(context.Message.UserId), context.CancellationToken);
    }

    private static async Task<bool> PersistIntoDb(AuthDbContext dbContext, User userEntity, CancellationToken ct)
    {
        dbContext.Users.Add(userEntity);

        return await dbContext.SaveChangesAsync(ct) is 1;
    }

    private static User CreateNewUser(PersistNewUser context)
    {
        return new User { Email = context.Email, Id = context.UserId, PasswordHash = context.PasswordHash };
    }
}
