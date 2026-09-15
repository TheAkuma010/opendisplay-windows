using System.Text.Json;

namespace OpenDisplay.Protocol;

public static class MessageParser
{
    private const int MaximumJsonFrameSize = 32768;

    public static bool IsJson(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return false;
        }

        if (data.Length > MaximumJsonFrameSize)
        {
            return false;
        }

        if (data[0] != (byte)'{')
        {
            return false;
        }

        return !data.Contains((byte)0);
    }

    public static JsonDocument ParseJson(ReadOnlyMemory<byte> data)
    {
        return JsonDocument.Parse(data);
    }
}