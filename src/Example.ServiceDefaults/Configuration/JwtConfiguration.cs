namespace Example.ServiceDefaults.Configuration;

public sealed class JwtConfiguration
{
    public const string SectionName = "Jwt";

    public required string PublicKey { get; init; }
    public required string PrivateKey { get; init; }
    public required string Audience { get; init; }
    public required string Issuer { get; init; }

    public required TimeSpan AccessTokenValidity { get; init; }
    public required TimeSpan RefreshTokenValidity { get; init; }
}
