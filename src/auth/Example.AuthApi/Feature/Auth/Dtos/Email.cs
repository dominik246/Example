using FluentValidation;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Example.AuthApi.Feature.Auth.Dtos;

public sealed record EmailAddress(string Email)
{
    public static implicit operator string(EmailAddress obj) => obj.Email;
    public static implicit operator EmailAddress(string email) => new(email);
}

public sealed class EmailAddressValidator : Validator<EmailAddress>
{
    public EmailAddressValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Email address has to be defined.")
            .EmailAddress().WithMessage("Email address is not valid.")
            .MaximumLength(320).WithMessage("Email address can have maximum length of 320 characters.");
    }
}

public sealed class EmailAddressJsonConverter1 : Newtonsoft.Json.JsonConverter<EmailAddress>
{
    public override EmailAddress? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, EmailAddress? existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        return new EmailAddress(reader.ReadAsString()!);
    }

    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, EmailAddress? value, Newtonsoft.Json.JsonSerializer serializer)
    {
        writer.WriteValue(value!.Email);
    }
}

public sealed class EmailAddressJsonConverter2 : JsonConverter<EmailAddress>
{
    public override EmailAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new EmailAddress(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, EmailAddress value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value!.Email);
    }
}