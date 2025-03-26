namespace Example.MassTransit.RegisterNewUser.EventModels;

public sealed class PersistNewUser
{
    public required Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string EmailConfirmHash { get; set; }
    public required string PasswordHash { get; set; }
}
