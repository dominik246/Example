namespace Example.ServiceDefaults.Configuration;

public sealed class EmailConfiguration
{
    public static readonly string SectionName = "Email";

    public required string FromName { get; init; }
    public required string FromAddress { get; init; }
    public required Uri MailServerAddress { get; init; }
    public required string ApiKey { get; init; }
    public required TimeSpan SendEmailCooldown { get; init; }

    public required Templates Templates { get; init; }
    public required Subjects Subjects { get; init; }
}

public sealed class Templates
{
    public required string PasswordRecovery { get; init; }
    public required string EmailConfirm { get; init; }
}

public sealed class Subjects
{
    public required string PasswordRecovery { get; init; }
    public required string EmailConfirm { get; init; }
}