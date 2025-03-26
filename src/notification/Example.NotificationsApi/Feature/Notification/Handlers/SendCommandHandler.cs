using Example.Api.Base.Consts;
using Example.Database.Base.Enums;
using Example.NotificationsApi.Database;
using Example.NotificationsApi.Database.Models;

using FastEndpoints.Security;

namespace Example.NotificationsApi.Feature.Notification.Handlers;

public sealed record SendCommand(List<Guid> UserIds, List<Guid> GroupIds, string Message, NotificationSeverity Severity) : ICommand<bool>;
public sealed class SendCommandHandler(NotificationDbContext db, IHttpContextAccessor accessor) : CommandHandler<SendCommand, bool>
{
    public override async Task<bool> ExecuteAsync(SendCommand command, CancellationToken ct = default)
    {
        var issuerId = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.UserId);

        if (!Guid.TryParse(issuerId, out var parsedIssuerId))
        {
            AddError("Auth mismatch.", "401");
            return false;
        }

        var newNotification = new Database.Models.Notification
        {
            Severity = command.Severity,
            Message = command.Message,
            IssuedByUserId = parsedIssuerId
        };

        if (command.UserIds is { Count: > 0 })
        {
            var userNotifications = command.UserIds.ConvertAll(p => new UserNotification
            {
                Notification = newNotification,
                RecieverId = p,
                Status = NotificationStatus.Unread,
                NotificationRecieverType = NotificationRecieverType.User
            });

            db.UserNotifications.UpdateRange(userNotifications);
        }

        if (command.GroupIds is { Count: >= 0 })
        {
            var userNotifications = command.GroupIds.ConvertAll(p => new UserNotification
            {
                Notification = newNotification,
                RecieverId = p,
                Status = NotificationStatus.Unread,
                NotificationRecieverType = NotificationRecieverType.Group
            });

            db.UserNotifications.UpdateRange(userNotifications);
        }

        await db.SaveChangesAsync(ct);

        return true;
    }
}