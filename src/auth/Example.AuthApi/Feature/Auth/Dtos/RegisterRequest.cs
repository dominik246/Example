namespace Example.AuthApi.Feature.Auth.Dtos;

public sealed class RegisterRequest
{
    /// <summary>
    /// The user's email address which acts as a user name.
    /// </summary>
    public required EmailAddress Email { get; init; }

    /// <summary>
    /// The user's password.
    /// </summary>
    public required AuthPassword Password { get; init; }
}

public sealed class RegisterRequestValidator : Validator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(p => p.Email).SetValidator(new EmailAddressValidator());
        RuleFor(p => p.Password).SetValidator(new AuthPasswordValidator());
    }
}