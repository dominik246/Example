namespace Example.ServiceDefaults.Configuration;

public sealed class NatsConfiguration
{
    public static readonly string SectionName = "nats";

    public required string Username { get; init; }
    public required string Password { get; init; }
}
