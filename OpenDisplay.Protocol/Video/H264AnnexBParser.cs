namespace OpenDisplay.Protocol.Video;

public enum H264NalType
{
    Unspecified = 0,
    NonIdrSlice = 1,
    IdrSlice = 5,
    Sei = 6,
    Sps = 7,
    Pps = 8
}

public sealed class H264NalUnit
{
    public H264NalType Type { get; }

    public ReadOnlyMemory<byte> Data { get; }

    public H264NalUnit(
        H264NalType type,
        ReadOnlyMemory<byte> data)
    {
        Type = type;
        Data = data;
    }
}

public static class H264AnnexBParser
{
    public static IReadOnlyList<H264NalUnit> Parse(
        ReadOnlyMemory<byte> data)
    {
        var result =
            new List<H264NalUnit>();

        var span = data.Span;

        var firstStart =
            FindStartCode(span, 0);

        if (firstStart < 0)
        {
            return result;
        }

        var currentStart = firstStart;

        while (currentStart >= 0)
        {
            var startCodeLength =
                GetStartCodeLength(
                    span,
                    currentStart
                );

            var nalStart =
                currentStart + startCodeLength;

            var nextStart =
                FindStartCode(
                    span,
                    nalStart
                );

            var nalEnd =
                nextStart >= 0
                    ? nextStart
                    : span.Length;

            if (nalEnd > nalStart)
            {
                var nal =
                    data.Slice(
                        nalStart,
                        nalEnd - nalStart
                    );

                var nalType =
                    GetNalType(nal.Span);

                result.Add(
                    new H264NalUnit(
                        nalType,
                        nal
                    )
                );
            }

            currentStart = nextStart;
        }

        return result;
    }

    private static H264NalType GetNalType(
        ReadOnlySpan<byte> nal)
    {
        if (nal.IsEmpty)
        {
            return H264NalType.Unspecified;
        }

        return (H264NalType)(nal[0] & 0x1F);
    }

    private static int FindStartCode(
        ReadOnlySpan<byte> data,
        int offset)
    {
        for (
            var i = offset;
            i <= data.Length - 3;
            i++)
        {
            if (
                data[i] == 0x00 &&
                data[i + 1] == 0x00 &&
                data[i + 2] == 0x01)
            {
                return i;
            }
        }

        return -1;
    }

    private static int GetStartCodeLength(
        ReadOnlySpan<byte> data,
        int offset)
    {
        if (
            offset + 4 <= data.Length &&
            data[offset] == 0x00 &&
            data[offset + 1] == 0x00 &&
            data[offset + 2] == 0x00 &&
            data[offset + 3] == 0x01)
        {
            return 4;
        }

        return 3;
    }
}