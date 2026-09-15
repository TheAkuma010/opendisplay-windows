using System.Text.Json;

namespace OpenDisplay.Windows.Protocol;

public static class MessageParser
{
    public static bool IsJson(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return false;
        }

        var first = data[0];

        return first == (byte)'{' ||
               first == (byte)'[';
    }

    public static JsonDocument ParseJson(ReadOnlyMemory<byte> data)
    {
        return JsonDocument.Parse(data);
    }
}