namespace Example.MassTransit.PasswordRecovery.EventModels;

public sealed class PasswordRecoveryPersist
{
    public required Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string Hash { get; set; }
}