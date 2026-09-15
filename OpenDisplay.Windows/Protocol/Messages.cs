using System.Text.Json.Serialization;

namespace OpenDisplay.Windows.Protocol;

public class HelloMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "hello";

    [JsonPropertyName("pv")]
    public int ProtocolVersion { get; set; } = 3;
}

public class WelcomeMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "welcome";

    [JsonPropertyName("pv")]
    public int ProtocolVersion { get; set; } = 3;
}