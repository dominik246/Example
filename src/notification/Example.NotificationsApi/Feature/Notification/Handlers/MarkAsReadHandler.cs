using Example.NotificationsApi.Database;
using Example.ServiceDefaults.Consts;

using FastEndpoints.Security;

using Microsoft.EntityFrameworkCore;

namespace Example.NotificationsApi.Feature.Notification.Handlers;

public sealed record MarkAsReadCommand(Guid NotificationId) : ICommand<bool>;
public sealed class MarkAsReadHandler(NotificationDbContext db, IHttpContextAccessor accessor) : CommandHandler<MarkAsReadCommand, bool>
{
    public override async Task<bool> ExecuteAsync(MarkAsReadCommand command, CancellationToken ct = default)
    {
        var userId = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.UserId);

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            AddError("Auth mismatch.", "401");
            return false;
        }

        var dbResult = await db.UserNotifications.FirstOrDefaultAsync(p => p.RecieverId == parsedUserId && p.NotificationId == command.NotificationId, ct);

        if (dbResult is null)
        {
            AddError("Notification not found.", "404");
            return false;
        }

        dbResult.Status = Database.Enums.NotificationStatus.Read;
        db.UserNotifications.Update(dbResult);

        return await db.SaveChangesAsync(ct) is 1;
    }
}