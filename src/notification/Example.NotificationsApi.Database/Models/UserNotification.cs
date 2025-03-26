using Example.Database.Base.BaseModels;
using Example.Database.Base.Enums;

namespace Example.NotificationsApi.Database.Models;

public sealed class UserNotification : BaseEntity
{
    public Guid RecieverId { get; set; }
    public NotificationRecieverType NotificationRecieverType { get; set; }

    public Guid NotificationId { get; set; }
    public Notification? Notification { get; set; }

    public NotificationStatus Status { get; set; }
}
