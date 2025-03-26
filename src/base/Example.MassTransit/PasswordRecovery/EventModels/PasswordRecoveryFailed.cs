namespace Example.MassTransit.PasswordRecovery.EventModels;

public sealed class PasswordRecoveryInitialFailed : FailedEvent;
public sealed class PasswordRecoveryPersistToDbFailed : FailedEvent;
public sealed class PasswordRecoveryEmailSendFailed : FailedEvent;
