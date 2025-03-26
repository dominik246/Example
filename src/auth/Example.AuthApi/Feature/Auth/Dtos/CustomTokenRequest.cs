using FastEndpoints.Security;

using FluentValidation;

namespace Example.AuthApi.Feature.Auth.Dtos;

public sealed class CustomTokenRequest : TokenRequest
{
    public string AccessToken { get; init; } = default!;
}

public sealed class CustomTokenRequestValidator : Validator<CustomTokenRequest>
{
    public CustomTokenRequestValidator()
    {
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("UserId has to be set.")
            .Must(p => Guid.TryParse(p, out _)).WithMessage("UserId has to be valid.");

        RuleFor(p => p.RefreshToken).NotEmpty().WithMessage("Refresh Token has to be set.")
            .Length(AuthConsts.RefreshTokenHexLength).WithMessage("Refresh token is not valid.");
        RuleFor(p => p.AccessToken).NotEmpty().WithMessage("Access Token has to be set.");
    }
}