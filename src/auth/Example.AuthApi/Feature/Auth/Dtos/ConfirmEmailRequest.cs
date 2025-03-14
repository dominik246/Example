using FluentValidation;

using Example.ServiceDefaults.Consts;

namespace Example.AuthApi.Feature.Auth.Dtos;

public sealed class ConfirmEmailRequest
{
    [QueryParam]
    public required string Hash { get; init; }

    [FromQuery]
    public required EmailAddress Email { get; init; }
}

public sealed class ConfirmEmailRequestValidator : Validator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(p => p.Hash).NotEmpty().WithMessage("Hash has to be set.")
            .Length(AuthConsts.ConfirmEmailHexLength).WithMessage("Hash is not valid.");
        RuleFor(p => p.Email).SetValidator(new EmailAddressValidator());
    }
}