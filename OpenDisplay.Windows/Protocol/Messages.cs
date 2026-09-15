using System.Text.Json.Serialization;

namespace OpenDisplay.Windows.Protocol;

public class HelloMessage
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "hello";

    [JsonPropertyName("pixelsWide")]
    public int PixelsWide { get; init; }

    [JsonPropertyName("pixelsHigh")]
    public int PixelsHigh { get; init; }

    [JsonPropertyName("scale")]
    public double Scale { get; init; }

    [JsonPropertyName("device")]
    public string? Device { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("pv")]
    public int ProtocolVersion { get; init; }
}

public class WelcomeMessage
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "welcome";

    [JsonPropertyName("pv")]
    public int ProtocolVersion { get; init; }

    [JsonPropertyName("min")]
    public int MinimumProtocolVersion { get; init; }
}

public class PingMessage
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "ping";

    [JsonPropertyName("t")]
    public long Timestamp { get; init; }
}

public class PongMessage
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "pong";

    [JsonPropertyName("t")]
    public long Timestamp { get; init; }

    [JsonPropertyName("mt")]
    public long SenderTimestamp { get; init; }
}