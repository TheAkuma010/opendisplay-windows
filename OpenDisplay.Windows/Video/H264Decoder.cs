using OpenDisplay.Protocol.Video;

namespace OpenDisplay.Windows.Video;

public interface IH264Decoder : IDisposable
{
    IEnumerable<VideoFrame> Decode(
        H264AccessUnit accessUnit);
}