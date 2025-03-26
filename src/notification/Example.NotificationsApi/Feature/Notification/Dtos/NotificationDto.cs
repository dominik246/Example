using Example.Database.Base.Enums;

namespace Example.NotificationsApi.Feature.Notification.Dtos;

public sealed record NotificationDto(Guid Id, string Message, Guid CreatedByUserId, NotificationSeverity Severity, NotificationStatus Status, DateTimeOffset DateCreated);
