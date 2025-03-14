namespace Example.ServiceDefaults.Configuration;

public sealed class SeqConfiguration
{
    public static readonly string SectionName = "SeqConfiguration";

    public required string DataLocation { get; init; }
}
