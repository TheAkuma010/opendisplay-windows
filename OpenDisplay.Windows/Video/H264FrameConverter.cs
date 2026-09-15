using FFmpeg.AutoGen;

namespace OpenDisplay.Windows.Video;

public unsafe sealed class H264FrameConverter : IDisposable
{
    private SwsContext* _swsContext;

    private int _width;
    private int _height;

    public VideoFrame Convert(AVFrame* frame)
    {
        if (frame == null)
        {
            throw new ArgumentNullException(nameof(frame));
        }

        if (frame->width <= 0 || frame->height <= 0)
        {
            throw new InvalidOperationException(
                "AVFrame possui dimensões inválidas."
            );
        }

        EnsureContext(
            frame->width,
            frame->height,
            (AVPixelFormat)frame->format
        );

        var stride =
            frame->width * 4;

        var data =
            new byte[
                stride *
                frame->height
            ];

        fixed (byte* destination = data)
        {
            var destinationData =
                new byte_ptrArray4
                {
                    [0] = destination
                };

            var destinationStride =
                new int_array4
                {
                    [0] = stride
                };

            var sourceData =
                new byte_ptrArray8
                {
                    [0] = frame->data[0],
                    [1] = frame->data[1],
                    [2] = frame->data[2]
                };

            var sourceStride =
                new int_array8
                {
                    [0] = frame->linesize[0],
                    [1] = frame->linesize[1],
                    [2] = frame->linesize[2]
                };

            ffmpeg.sws_scale(
                _swsContext,
                sourceData,
                sourceStride,
                0,
                frame->height,
                destinationData,
                destinationStride
            );
        }

        return new VideoFrame(
            frame->width,
            frame->height,
            stride,
            data
        );
    }

    private void EnsureContext(
        int width,
        int height,
        AVPixelFormat sourceFormat)
    {
        if (
            _swsContext != null &&
            _width == width &&
            _height == height
        )
        {
            return;
        }

        if (_swsContext != null)
        {
            ffmpeg.sws_freeContext(
                _swsContext
            );

            _swsContext = null;
        }

        _swsContext =
            ffmpeg.sws_getContext(
                width,
                height,
                sourceFormat,
                width,
                height,
                AVPixelFormat.AV_PIX_FMT_BGRA,
                (int)SwsFlags.SWS_BILINEAR,
                null,
                null,
                null
            );

        if (_swsContext == null)
        {
            throw new InvalidOperationException(
                "Não foi possível criar o contexto de conversão de vídeo."
            );
        }

        _width = width;
        _height = height;
    }

    public void Dispose()
    {
        if (_swsContext != null)
        {
            ffmpeg.sws_freeContext(
                _swsContext
            );

            _swsContext = null;
        }
    }
}