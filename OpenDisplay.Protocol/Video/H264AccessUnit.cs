namespace OpenDisplay.Protocol.Video;

public sealed class H264AccessUnit
{
    public IReadOnlyList<H264NalUnit> NalUnits { get; }

    public bool IsKeyFrame { get; }

    public H264AccessUnit(
        IReadOnlyList<H264NalUnit> nalUnits,
        bool isKeyFrame)
    {
        NalUnits = nalUnits;
        IsKeyFrame = isKeyFrame;
    }

    public byte[] ToAnnexB()
    {
        using var stream = new MemoryStream();

        foreach (var nal in NalUnits)
        {
            stream.Write(
                new byte[]
                {
                    0x00,
                    0x00,
                    0x00,
                    0x01
                }
            );

            stream.Write(nal.Data.Span);
        }

        return stream.ToArray();
    }
}