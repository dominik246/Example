using Example.MassTransit.SendEmail;

namespace Example.MassTransit.PasswordRecovery.EventModels;

public sealed record PasswordRecoveryEmailSentCompleted : SendEmailCompleted;