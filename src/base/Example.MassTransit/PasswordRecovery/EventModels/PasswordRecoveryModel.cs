namespace Example.MassTransit.PasswordRecovery.EventModels;

public sealed class PasswordRecoveryModel
{
    public required Guid UserId { get; set; }
    public required string Hash { get; set; }
    public required string Email { get; set; }
    public required string EmailTemplate { get; set; }
    public required string EmailSubject { get; set; }
}
