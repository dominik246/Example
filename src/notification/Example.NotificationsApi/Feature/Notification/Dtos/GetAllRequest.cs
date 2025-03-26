using Example.Database.Base.Enums;

namespace Example.NotificationsApi.Feature.Notification.Dtos;

public sealed record GetAllRequest(DateTimeOffset? StartDate, DateTimeOffset? EndDate, NotificationStatus? Status);