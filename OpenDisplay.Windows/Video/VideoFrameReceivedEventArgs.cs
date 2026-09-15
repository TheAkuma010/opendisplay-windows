namespace OpenDisplay.Windows.Video;

public sealed class VideoFrameReceivedEventArgs : EventArgs
{
    public VideoFrame Frame { get; }

    public VideoFrameReceivedEventArgs(VideoFrame frame)
    {
        Frame = frame;
    }
}