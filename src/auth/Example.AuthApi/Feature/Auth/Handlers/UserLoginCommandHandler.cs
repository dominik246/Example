using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;

namespace Example.AuthApi.Feature.Auth.Handlers;

public sealed record UserLoginCommand(string Email, string Password) : ICommand<User?>;

public sealed class UserLoginCommandHandler(AuthDbContext dbContext) : CommandHandler<UserLoginCommand, User?>
{
    public override async Task<User?> ExecuteAsync(UserLoginCommand command, CancellationToken ct)
    {
        var dbResponse = await dbContext.Users.Include(p => p.UserGroups).FirstOrDefaultAsync(x => x.Email == command.Email, ct);

        if (dbResponse is null)
        {
            AddError(x => x.Email, "User not found.", "404");
            return null;
        }

        var verificationResult = new PasswordHasher<User>().VerifyHashedPassword(dbResponse, dbResponse.PasswordHash, command.Password);

        if (verificationResult is not PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)
        {
            AddError(x => x.Password, "Password is not valid.", "401");
            return null;
        }

        if (!dbResponse.EmailConfirmed)
        {
            AddError(x => x.Email, "Email address is not confirmed.", "401");
            return null;
        }

        if (dbResponse.IsDisabled)
        {
            AddError(x => x.Email, "User is disabled.", "401");
            return null;
        }

        return dbResponse;
    }
}