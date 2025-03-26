using FluentValidation;

namespace Example.AuthApi.Feature.Auth.Dtos;

public sealed record ConfirmPasswordRecoveryRequest(EmailAddress Email, string SecurityCode, AuthPassword NewPassword);

public sealed class RequestValidator : Validator<ConfirmPasswordRecoveryRequest>
{
    public RequestValidator()
    {
        RuleFor(p => p.Email).SetValidator(new EmailAddressValidator());
        RuleFor(p => p.SecurityCode).NotEmpty().WithMessage("Security code has to be defined.")
            .Length(AuthConsts.PasswordRecoveryHexLength).WithMessage("Security code is not valid.");
        RuleFor(p => p.NewPassword).SetValidator(new AuthPasswordValidator());
    }
}