namespace Example.MassTransit.PasswordRecovery.EventModels;

public sealed class PasswordRecoveryPersistToDbCompleted
{
    public required Guid UserId { get; set; }
}
