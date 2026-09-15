namespace OpenDisplay.Protocol.Video;

public sealed class H264AccessUnitAssembler
{
    private readonly List<H264NalUnit> _current = [];

    public IReadOnlyList<H264AccessUnit> Add(
        H264NalUnit nal)
    {
        var completed = new List<H264AccessUnit>();

        switch (nal.Type)
        {
            case H264NalType.Sps:
            case H264NalType.Pps:
            case H264NalType.Sei:
                _current.Add(nal);
                break;

            case H264NalType.IdrSlice:
                if (HasVideoSlice())
                {
                    completed.Add(
                        CompleteCurrentAccessUnit()
                    );
                }

                _current.Add(nal);

                break;

            case H264NalType.NonIdrSlice:
                if (HasVideoSlice())
                {
                    completed.Add(
                        CompleteCurrentAccessUnit()
                    );
                }

                _current.Add(nal);

                break;

            default:
                _current.Add(nal);
                break;
        }

        return completed;
    }

    public H264AccessUnit? Flush()
    {
        if (_current.Count == 0)
        {
            return null;
        }

        return CompleteCurrentAccessUnit();
    }

    private bool HasVideoSlice()
    {
        return _current.Any(
            nal =>
                nal.Type == H264NalType.IdrSlice ||
                nal.Type == H264NalType.NonIdrSlice
        );
    }

    private H264AccessUnit CompleteCurrentAccessUnit()
    {
        var nalUnits =
            _current.ToArray();

        _current.Clear();

        var isKeyFrame =
            nalUnits.Any(
                nal =>
                    nal.Type == H264NalType.IdrSlice
            );

        return new H264AccessUnit(
            nalUnits,
            isKeyFrame
        );
    }
}