using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.ServiceDefaults.Consts;

using FastEndpoints.Security;

using Microsoft.EntityFrameworkCore;

namespace Example.AuthApi.Feature.Users.Handlers;

public sealed record UpdateUserCommand(Guid Id, bool IsDisabled, bool IsEmailConfirmed) : ICommand<bool>;
public sealed class UpdateUserCommandHandler(AuthDbContext db, IHttpContextAccessor accessor) : CommandHandler<UpdateUserCommand, bool>
{
    public override async Task<bool> ExecuteAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var dbEntry = await db.Users.FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (dbEntry is null)
        {
            AddError(p => p.Id, "User does not exist.", "404");
            return false;
        }

        var userId = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.UserId);
        if (string.IsNullOrWhiteSpace(userId) || Guid.TryParse(userId, out var authGuid))
        {
            AddError("Auth missmatch", "401");
            return false;
        }

        if (command.Id == authGuid)
        {
            return await UpdateSelf(dbEntry, command, db, ct);
        }

        var isAdmin = await db.UserGroups.AnyAsync(p => p.UserId == authGuid && p.Group!.IsAdminGroup, ct);

        if (isAdmin)
        {
            return await AdminUpdate(dbEntry, command, db, ct);
        }

        return false;
    }

    private static Task<bool> UpdateSelf(User dbEntry, UpdateUserCommand command, AuthDbContext db, CancellationToken ct)
    {
        // currently nothing to update on user :)
        return Task.FromResult(true);
    }

    private static async Task<bool> AdminUpdate(User dbEntry, UpdateUserCommand command, AuthDbContext db, CancellationToken ct)
    {
        dbEntry.IsDisabled = command.IsDisabled;
        dbEntry.EmailConfirmed = command.IsEmailConfirmed;

        db.Users.Update(dbEntry);

        return await db.SaveChangesAsync(ct) is 1;
    }
}