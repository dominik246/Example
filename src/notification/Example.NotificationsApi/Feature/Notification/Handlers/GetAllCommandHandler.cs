using Example.NotificationsApi.Database;
using Example.NotificationsApi.Database.Enums;
using Example.NotificationsApi.Database.Models;
using Example.NotificationsApi.Feature.Notification.Dtos;
using Example.ServiceDefaults.Consts;

using FastEndpoints.Security;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace Example.NotificationsApi.Feature.Notification.Handlers;

public sealed record GetAllCommand(NotificationStatus? Status, DateTimeOffset? StartDate, DateTimeOffset? EndDate) : ICommand<GetAllResponse?>;
public sealed class GetAllCommandHandler(NotificationDbContext db, IHttpContextAccessor accessor) : CommandHandler<GetAllCommand, GetAllResponse?>
{
    public override async Task<GetAllResponse?> ExecuteAsync(GetAllCommand command, CancellationToken ct = default)
    {
        var groupsFromToken = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.Groups) ?? "[]";
        var userId = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.UserId);

        var parsedGroupIds = JsonSerializer.Deserialize<List<Guid>>(groupsFromToken);
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            AddError("Auth Mismatch", "401");
            return null;
        }

        IQueryable<UserNotification> query = db.UserNotifications;

        if (command.Status.HasValue)
        {
            query = query.Where(p => p.Status == command.Status.Value);
        }

        if (command.StartDate.HasValue)
        {
            query = query.Where(p => p.DateCreated >= command.StartDate.Value);
        }

        if (command.EndDate.HasValue)
        {
            query = query.Where(p => p.DateCreated <= command.EndDate.Value);
        }

        if (parsedGroupIds is { Count: > 0 })
        {
            query = query.Where(p => p.RecieverId == parsedUserId || parsedGroupIds.Contains(p.RecieverId));
        }
        else
        {
            query = query.Where(p => p.RecieverId == parsedUserId);
        }

        var dbResult = await query
            .Include(p => p.Notification)
            .GroupBy(p => p.NotificationId)
            .Select(p => p
                .First(x =>
                    (x.NotificationRecieverType == NotificationRecieverType.User && x.Status == NotificationStatus.Unread)
                    || (x.NotificationRecieverType == NotificationRecieverType.Group && x.Status == NotificationStatus.Unread)
                    || x.NotificationRecieverType == NotificationRecieverType.User
                    || x.NotificationRecieverType == NotificationRecieverType.Group))
            .ToListAsync(ct);

        var mapped = dbResult.Select(p => new NotificationDto(p.Notification!.Id, p.Notification.Message, p.Notification.IssuedByUserId, p.Notification.Severity, p.Status, p.Notification.DateCreated));

        return new(mapped.ToList());
    }
}
