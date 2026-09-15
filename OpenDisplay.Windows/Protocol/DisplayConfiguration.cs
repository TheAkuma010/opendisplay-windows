namespace OpenDisplay.Windows.Protocol;

public class DisplayConfiguration
{
    public int PixelsWide { get; init; } = 1920;

    public int PixelsHigh { get; init; } = 1080;

    public double Scale { get; init; } = 1.0;

    public string Device { get; init; } = "Windows";

    public int ProtocolVersion { get; init; } = 3;
}