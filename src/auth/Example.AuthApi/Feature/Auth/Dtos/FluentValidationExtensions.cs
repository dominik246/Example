using FluentValidation;

using System.Text.RegularExpressions;

namespace Example.AuthApi.Feature.Auth.Dtos;

public static partial class FluentValidationExtensions
{
    [GeneratedRegex("[A-Z]", RegexOptions.None, 1000)]
    public static partial Regex ContainsAtLeastOneCapitalLetterRegex();

    [GeneratedRegex("[a-z]", RegexOptions.None, 1000)]
    public static partial Regex ContainsAtLeastOneLowercaseLetterRegex();

    [GeneratedRegex("[0-9]", RegexOptions.None, 1000)]
    public static partial Regex ContainsAtLeastOneNumberRegex();

    [GeneratedRegex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]", RegexOptions.None, 1000)]
    public static partial Regex ContainsAtLeastOneSpecialCharacterRegex();

    public static IRuleBuilderOptions<T, string> ContainsCapitalLetter<T>(this IRuleBuilderOptions<T, string> builder)
    {
        return builder.Must(p => ContainsAtLeastOneCapitalLetterRegex().IsMatch(p));
    }

    public static IRuleBuilderOptions<T, string> ContainsLowercaseLetter<T>(this IRuleBuilderOptions<T, string> builder)
    {
        return builder.Must(p => ContainsAtLeastOneLowercaseLetterRegex().IsMatch(p));
    }

    public static IRuleBuilderOptions<T, string> ContainsNumber<T>(this IRuleBuilderOptions<T, string> builder)
    {
        return builder.Must(p => ContainsAtLeastOneNumberRegex().IsMatch(p));
    }

    public static IRuleBuilderOptions<T, string> ContainsSpecialCharacter<T>(this IRuleBuilderOptions<T, string> builder)
    {
        return builder.Must(p => ContainsAtLeastOneSpecialCharacterRegex().IsMatch(p));
    }
}