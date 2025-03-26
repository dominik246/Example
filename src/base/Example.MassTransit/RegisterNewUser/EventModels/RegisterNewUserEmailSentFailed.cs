using Example.MassTransit.SendEmail;

namespace Example.MassTransit.RegisterNewUser.EventModels;

public sealed class RegisterNewUserFailed : FailedEvent;
public sealed class RegisterNewUserPersistToDbFailed : FailedEvent;
public sealed class RegisterNewUserEmailSendFailed : FailedEvent;