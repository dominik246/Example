using Example.ServiceDefaults.Configuration;
using Example.ServiceDefaults.Models;

using Microsoft.Extensions.Options;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace Example.EmailService.Feature.SendEmail;

public sealed class SendEmailClient(HttpClient httpClient, IOptions<EmailConfiguration> configuration)
{
    public async Task<bool> SendAsync(MailAddressModel values, CancellationToken token)
    {
        using var message = ConstructMessage(configuration.Value, values);

        var result = await httpClient.SendAsync(message, token);

        return result.IsSuccessStatusCode;
    }

    private static HttpRequestMessage ConstructMessage(EmailConfiguration config, MailAddressModel values)
    {
        var form = new Dictionary<string, string>
        {
            ["from"] = $"{config.FromName} <{config.FromAddress}>",
            ["to"] = values.SendTo,
            ["subject"] = values.Subject,
            ["template"] = values.Template,
            ["t:variables"] = JsonSerializer.Serialize(values.Context),
        };

        return new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = config.MailServerAddress,
            Content = new FormUrlEncodedContent(form),
            Headers = { Authorization = new AuthenticationHeaderValue("Basic", GetBase64ApiKey(config.ApiKey)) }
        };
    }

    private static string GetBase64ApiKey(string apiKey)
    {
        var authValue = $"api:{apiKey}";
        const int maxSpanLength = 1024;
        var spanLength = authValue.Length;

        Span<byte> authValueBytes = maxSpanLength > spanLength ? stackalloc byte[spanLength] : new byte[spanLength];

        var op = Utf8.FromUtf16(authValue, authValueBytes, out _, out var written);
        if (op is System.Buffers.OperationStatus.Done)
        {
            return Convert.ToBase64String(authValueBytes[..written]);
        }

        var bytes = Encoding.UTF8.GetBytes(authValue);
        return Convert.ToBase64String(bytes);
    }
}
