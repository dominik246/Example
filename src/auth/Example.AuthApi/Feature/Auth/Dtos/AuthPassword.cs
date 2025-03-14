using FluentValidation;

using System.Text.Json;
using System.Text.Json.Serialization;

using Example.ApiService.Feature.Dtos;

namespace Example.AuthApi.Feature.Auth.Dtos;

public sealed record AuthPassword(string Password)
{
    public static implicit operator string(AuthPassword obj) => obj.Password;
    public static implicit operator AuthPassword(string password) => new(password);
}

public sealed class AuthPasswordValidator : Validator<AuthPassword>
{
    public AuthPasswordValidator()
    {
        RuleFor(x => x.Password)
            .Length(8, 128).WithMessage("Has to be at at least 8 characters long and at most 128.")
            .ContainsCapitalLetter().WithMessage("Password has to contain at least one capital letter.")
            .ContainsLowercaseLetter().WithMessage("Password has to contain at least one lowercase letter.")
            .ContainsSpecialCharacter().WithMessage("Password has to contain at least one special character.")
            .ContainsNumber().WithMessage("Password has to contain at least one numerical value.");
    }
}

public sealed class AuthPasswordJsonConverter1 : Newtonsoft.Json.JsonConverter<AuthPassword>
{
    public override AuthPassword? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, AuthPassword? existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        return new AuthPassword(reader.ReadAsString()!);
    }

    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, AuthPassword? value, Newtonsoft.Json.JsonSerializer serializer)
    {
        writer.WriteValue(value!.Password);
    }
}

public sealed class AuthPasswordJsonConverter2 : JsonConverter<AuthPassword>
{
    public override AuthPassword? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new AuthPassword(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, AuthPassword value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value!.Password);
    }
}