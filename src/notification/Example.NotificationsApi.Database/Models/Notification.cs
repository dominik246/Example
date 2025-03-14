using System.ComponentModel.DataAnnotations;

using Example.Database.Base.BaseModels;
using Example.NotificationsApi.Database.Enums;

namespace Example.NotificationsApi.Database.Models;

public sealed class Notification : BaseEntity<Guid>
{
    [MaxLength(1000)]
    public string Message { get; set; } = default!;

    public NotificationSeverity Severity { get; set; }

    public Guid IssuedByUserId { get; set; }
}
