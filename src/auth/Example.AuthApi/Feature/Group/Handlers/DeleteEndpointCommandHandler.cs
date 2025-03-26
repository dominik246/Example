using Example.Api.Base.Consts;
using Example.AuthApi.Database;

using FastEndpoints.Security;

using Microsoft.EntityFrameworkCore;

namespace Example.AuthApi.Feature.Group.Handlers;

public sealed record DeleteEndpointCommand(Guid Id) : ICommand<bool>;
public sealed class DeleteEndpointCommandHandler(AuthDbContext db, IHttpContextAccessor accessor) : CommandHandler<DeleteEndpointCommand, bool>
{
    public override async Task<bool> ExecuteAsync(DeleteEndpointCommand command, CancellationToken ct = default)
    {
        var dbResult = await db.Groups.FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (dbResult is null)
        {
            AddError("Group does not exist", "404");
            return false;
        }

        var userId = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.UserId);

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            AddError("Auth Mismatch", "400");
            return false;
        }

        var isUserAdmin = await db.UserGroups.AnyAsync(p => p.UserId == parsedUserId && p.Group!.IsAdminGroup, ct);
        if (!isUserAdmin && dbResult.IsAdminGroup)
        {
            AddError("User cannot delete an Admin group as the user is not an admin itself.", "401");
            return false;
        }

        var userGroups = await db.UserGroups.Where(p => p.GroupId == command.Id).ToListAsync(ct);

        db.Groups.Remove(dbResult);
        db.UserGroups.RemoveRange(userGroups);

        return await db.SaveChangesAsync(ct) == userGroups.Count + 1;
    }
}