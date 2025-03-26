namespace Example.MassTransit.RegisterNewUser.EventModels;

public sealed class RegisterNewUserModel
{
    public required Guid UserId { get; set; }
    public required string PasswordHash { get; set; }
    public required string Email { get; set; }
    public required string EmailConfirmHash { get; set; }
    public required string EmailTemplate { get; set; }
    public required string EmailSubject { get; set; }
}
