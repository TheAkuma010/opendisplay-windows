namespace OpenDisplay.Windows.Video;

public sealed class VideoFrame
{
    public int Width { get; }

    public int Height { get; }

    public int Stride { get; }

    public byte[] Data { get; }

    public VideoFrame(
        int width,
        int height,
        int stride,
        byte[] data)
    {
        Width = width;
        Height = height;
        Stride = stride;
        Data = data;
    }
}