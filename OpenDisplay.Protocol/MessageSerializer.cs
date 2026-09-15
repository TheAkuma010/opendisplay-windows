using System.Text;
using System.Text.Json;

namespace OpenDisplay.Protocol;

public static class MessageSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = null
    };

    public static byte[] Serialize<T>(T message)
    {
        var json = JsonSerializer.Serialize(message, Options);

        return Encoding.UTF8.GetBytes(json);
    }

    public static T? Deserialize<T>(ReadOnlySpan<byte> data)
    {
        return JsonSerializer.Deserialize<T>(data, Options);
    }
}