﻿using Example.Database.Base.Enums;

using System.Text.Json.Serialization;

namespace Example.NotificationsApi.Feature.Notification.Dtos;

public sealed record SendRequest(List<Guid> UserIds, List<Guid> GroupIds, string Message)
{
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationSeverity Severity { get; set; }
}
