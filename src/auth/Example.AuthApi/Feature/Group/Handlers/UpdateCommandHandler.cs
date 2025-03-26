using EntityFramework.Exceptions.Common;

using Example.Api.Base.Consts;
using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;

using FastEndpoints.Security;

using Microsoft.EntityFrameworkCore;

using AddErr = System.Action<System.Linq.Expressions.Expression<System.Func<Example.AuthApi.Feature.Group.Handlers.UpdateCommand, object?>>, string, string?>;

namespace Example.AuthApi.Feature.Group.Handlers;

public sealed record UpdateCommand(Guid Id, string Name, List<Guid> UserIds) : ICommand<bool>;
public sealed class UpdateCommandHandler(AuthDbContext db, IHttpContextAccessor accessor) : CommandHandler<UpdateCommand, bool>
{
    public override async Task<bool> ExecuteAsync(UpdateCommand command, CancellationToken ct = default)
    {
        Database.Models.Group? dbGroup = await db.Groups.Include(p => p.UserGroups).FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (dbGroup is null)
        {
            AddError(p => p.Id, "Group not found.", "404");
            return false;
        }

        var userId = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.UserId);

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            AddError("Auth Mismatch", "400");
            return false;
        }

        var isUserAdmin = await db.UserGroups.AnyAsync(p => p.UserId == parsedUserId && p.Group!.IsAdminGroup, ct);
        if (!isUserAdmin && dbGroup.IsAdminGroup)
        {
            AddError("User cannot modify an Admin group as the user is not an admin itself.", "401");
            return false;
        }

        if (dbGroup.UserGroups is not { Count: > 0 })
        {
            return await PersistAll(db, command, dbGroup, (a, b, c) => AddError(a, b, c), ct);
        }

        return await PersistDiff(db, command, dbGroup.UserGroups, dbGroup, (a, b, c) => AddError(a, b, c), ct);
    }

    private static async Task<bool> PersistAll(AuthDbContext db, UpdateCommand command, Database.Models.Group dbGroup, AddErr addError, CancellationToken ct)
    {
        var initialCount = 0;
        if (dbGroup.Name != command.Name)
        {
            dbGroup.Name = command.Name;
            db.Groups.Update(dbGroup);

            initialCount++;
        }

        var mapped = command.UserIds.ConvertAll(p => new UserGroup { GroupId = dbGroup.Id, UserId = p });
        db.UserGroups.AddRange(mapped);

        var result = 0;
        try
        {
            result = await db.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintException)
        {
            addError(p => p.Name, "Name is already used.", "400");
            return false;
        }

        return result == mapped.Count + initialCount;
    }

    private static async Task<bool> PersistDiff(AuthDbContext db, UpdateCommand command, ICollection<UserGroup> dbUserGroups, Database.Models.Group dbGroup, AddErr addError, CancellationToken ct)
    {
        var initialCount = 0;
        if (dbGroup.Name != command.Name)
        {
            dbGroup.Name = command.Name;
            db.Groups.Update(dbGroup);

            initialCount++;
        }

        var toDelete = dbUserGroups.ExceptBy(command.UserIds, p => p.UserId).ToList();
        var toAdd = command.UserIds.ExceptBy(dbUserGroups.Select(x => x.UserId), p => p)
            .Select(p => new UserGroup { UserId = p, GroupId = dbGroup.Id })
            .ToList();

        db.UserGroups.RemoveRange(toDelete);
        db.UserGroups.AddRange(toAdd);

        var result = 0;
        try
        {
            result = await db.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintException)
        {
            addError(p => p.Name, "Name is already used.", "400");
            return false;
        }

        return result == toDelete.Count + toAdd.Count + initialCount;
    }
}