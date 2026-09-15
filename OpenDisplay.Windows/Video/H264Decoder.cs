using FFmpeg.AutoGen;
using OpenDisplay.Protocol.Video;

namespace OpenDisplay.Windows.Video;

public unsafe sealed class H264Decoder : IDisposable
{
    private readonly AVCodec* _codec;
    private readonly AVCodecContext* _codecContext;
    private readonly AVFrame* _frame;
    private readonly AVPacket* _packet;

    private bool _disposed;

    public H264Decoder()
    {
        FFmpegLoader.Initialize();

        _codec = ffmpeg.avcodec_find_decoder(
            AVCodecID.AV_CODEC_ID_H264
        );

        if (_codec == null)
        {
            throw new InvalidOperationException(
                "Decoder H.264 não encontrado no FFmpeg."
            );
        }

        _codecContext =
            ffmpeg.avcodec_alloc_context3(_codec);

        if (_codecContext == null)
        {
            throw new InvalidOperationException(
                "Não foi possível criar AVCodecContext."
            );
        }

        var result =
            ffmpeg.avcodec_open2(
                _codecContext,
                _codec,
                null
            );

        if (result < 0)
        {
            throw new InvalidOperationException(
                $"Não foi possível abrir o decoder H.264. " +
                $"Código FFmpeg: {result}"
            );
        }

        _frame =
            ffmpeg.av_frame_alloc();

        _packet =
            ffmpeg.av_packet_alloc();

        if (_frame == null || _packet == null)
        {
            throw new InvalidOperationException(
                "Não foi possível alocar estruturas do FFmpeg."
            );
        }

        Console.WriteLine(
            "[FFmpeg] H.264 decoder inicializado."
        );
    }

    public void Decode(H264AccessUnit accessUnit)
    {
        var data = accessUnit.ToAnnexB();

        ffmpeg.av_packet_unref(_packet);

        fixed (byte* dataPtr = data)
        {
            _packet->data = dataPtr;
            _packet->size = data.Length;

            var sendResult =
                ffmpeg.avcodec_send_packet(
                    _codecContext,
                    _packet
                );

            if (sendResult < 0)
            {
                throw new InvalidOperationException(
                    $"avcodec_send_packet falhou. " +
                    $"Código FFmpeg: {sendResult}"
                );
            }

            while (true)
            {
                var receiveResult =
                    ffmpeg.avcodec_receive_frame(
                        _codecContext,
                        _frame
                    );

                if (
                    receiveResult ==
                    ffmpeg.AVERROR(ffmpeg.EAGAIN)
                )
                {
                    break;
                }

                if (
                    receiveResult ==
                    ffmpeg.AVERROR_EOF
                )
                {
                    break;
                }

                if (receiveResult < 0)
                {
                    throw new InvalidOperationException(
                        $"avcodec_receive_frame falhou. " +
                        $"Código FFmpeg: {receiveResult}"
                    );
                }

                Console.WriteLine(
                    $"[FFmpeg] Frame decodificado: " +
                    $"{_frame->width}x{_frame->height} " +
                    $"format={_frame->format}"
                );
            }

            _packet->data = null;
            _packet->size = 0;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_packet != null)
        {
            AVPacket* packet = _packet;
            ffmpeg.av_packet_free(&packet);
        }

        if (_frame != null)
        {
            AVFrame* frame = _frame;
            ffmpeg.av_frame_free(&frame);
        }

        if (_codecContext != null)
        {
            AVCodecContext* context = _codecContext;
            ffmpeg.avcodec_free_context(&context);
        }
    }
}