namespace Example.ServiceDefaults.Configuration;

public sealed class MqConfiguration
{
    public static readonly string SectionName = "Mq";

    public required string Username { get; init; }
    public required string Password { get; init; }
}
