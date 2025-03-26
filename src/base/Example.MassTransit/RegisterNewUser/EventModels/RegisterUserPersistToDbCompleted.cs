namespace Example.MassTransit.RegisterNewUser.EventModels;

public sealed record RegisterUserPersistToDbCompleted(Guid UserId);