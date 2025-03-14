namespace Example.ServiceDefaults.Configuration;

public class CacheConfiguration
{
    public int KeyChangeThreshold { get; init; }
    public TimeSpan PersistanceInterval { get; init; }
}

public sealed class AuthCacheConfiguration : CacheConfiguration
{
    public static readonly string SectionName = "AuthCache";

    public required ExpiryConfiguration ExpiryConfiguration { get; init; }
}

public sealed class ExpiryConfiguration
{
    public required TimeSpan UserExpiry { get; init; }
    public required TimeSpan UserPasswordRestoreExpiry { get; init; }
    public required TimeSpan UserTokenExpiry { get; init; }
    public required TimeSpan EmailConfirmExpiry { get; init; }
}