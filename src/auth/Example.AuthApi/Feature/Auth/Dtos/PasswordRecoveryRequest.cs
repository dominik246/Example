namespace Example.AuthApi.Feature.Auth.Dtos;

public sealed record PasswordRecoveryRequest(EmailAddress Email);

public sealed class PasswordRecoveryRequestValidator : Validator<PasswordRecoveryRequest>
{
    public PasswordRecoveryRequestValidator()
    {
        RuleFor(p => p.Email).SetValidator(new EmailAddressValidator());
    }
}