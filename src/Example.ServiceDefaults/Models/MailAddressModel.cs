using NATS.Client.Core;

using System.Buffers;
using System.Text;
using System.Text.Json;

using Example.ServiceDefaults.Defaults;

namespace Example.ServiceDefaults.Models;

public sealed record MailAddressModel(string Subject, string Template, string SendTo, IDictionary<string, string> Context);

public sealed class MailAddressModelSerializer : INatsSerialize<MailAddressModel>, INatsDeserialize<MailAddressModel>
{
    public MailAddressModel? Deserialize(in ReadOnlySequence<byte> buffer)
    {
        var json = Encoding.Default.GetString(buffer);
        return JsonSerializer.Deserialize<MailAddressModel>(json, JsonSerializerDefaultValues.NatsOptions);
    }

    public void Serialize(IBufferWriter<byte> bufferWriter, MailAddressModel value)
    {
        ReadOnlySpan<byte> serialized = JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerDefaultValues.NatsOptions);
        bufferWriter.Write(serialized);
    }
}
