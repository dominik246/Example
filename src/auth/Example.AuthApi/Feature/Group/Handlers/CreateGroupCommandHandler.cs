using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.ServiceDefaults.Consts;

using FastEndpoints.Security;

using Microsoft.EntityFrameworkCore;

namespace Example.AuthApi.Feature.Group.Handlers;

public sealed record CreateGroupCommand(string Name, bool IsAdminGroup, bool ShouldAutoAddSelf, List<Guid> UserIds) : ICommand<bool>;
public sealed class CreateGroupCommandHandler(AuthDbContext db, IHttpContextAccessor accessor) : CommandHandler<CreateGroupCommand, bool>
{
    public override async Task<bool> ExecuteAsync(CreateGroupCommand command, CancellationToken ct)
    {
        var existsInDb = await db.Groups.AnyAsync(x => x.Name == command.Name, ct);

        if (existsInDb)
        {
            AddError("Name already exists.", "400");
            return false;
        }

        var userId = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.UserId);

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            AddError("Auth Mismatch", "400");
            return false;
        }

        var isUserAdmin = await db.UserGroups.AnyAsync(p => p.UserId == parsedUserId && p.Group!.IsAdminGroup, ct);
        if (!isUserAdmin && command.IsAdminGroup)
        {
            AddError("User cannot create an Admin group as the user is not an admin itself.", "401");
            return false;
        }

        var iterator = 0;
        var group = new Database.Models.Group { Name = command.Name, IsAdminGroup = command.IsAdminGroup };

        if (command.ShouldAutoAddSelf && !command.UserIds.Contains(parsedUserId))
        {
            var userGroup = new UserGroup { Group = group, UserId = parsedUserId };

            db.UserGroups.Add(userGroup);
            iterator++;
        }

        if (command.UserIds.Count is 0)
        {
            return await PersistGroupIntoDb(db, group, iterator, ct);
        }

        return await PersistGroupWithUsersIntoDb(db, group, command.UserIds, iterator, ct);
    }

    private static async Task<bool> PersistGroupIntoDb(AuthDbContext db, Database.Models.Group group, int iterator, CancellationToken ct)
    {
        db.Groups.Add(group);

        return await db.SaveChangesAsync(ct) == iterator + 1;
    }

    private static async Task<bool> PersistGroupWithUsersIntoDb(AuthDbContext db, Database.Models.Group group, List<Guid> userIds, int iterator, CancellationToken ct)
    {
        var userGroups = userIds.ConvertAll(p => new UserGroup { Group = group, UserId = p });

        db.UserGroups.AddRange(userGroups);

        return await db.SaveChangesAsync(ct) == userIds.Count + iterator + 1;
    }
}