namespace Example.AuthApi.Feature.Auth.Dtos;

public sealed record LoginRequest(EmailAddress Email, AuthPassword Password);

public sealed class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(p => p.Email).SetValidator(new EmailAddressValidator());
        RuleFor(p => p.Password).SetValidator(new AuthPasswordValidator());
    }
}