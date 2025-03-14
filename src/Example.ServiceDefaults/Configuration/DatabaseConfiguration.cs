namespace Example.ServiceDefaults.Configuration;

public sealed class DatabaseConfiguration
{
    public static readonly string AuthSectionName = "AuthDatabase";
    public static readonly string NotificationSectionName = "NotificationDatabase";
    public string? Username { get; init; }
    public required string Password { get; init; }
}