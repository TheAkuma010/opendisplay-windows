using System.Buffers.Binary;
using System.Net.Sockets;

namespace OpenDisplay.Protocol;

public class FrameWriter
{
    private const int MaximumControlFrameSize = (1 << 20) - 1;

    private readonly NetworkStream _stream;

    public FrameWriter(NetworkStream stream)
    {
        _stream = stream;
    }

    public async Task WriteFrameAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        if (payload.Length == 0)
        {
            throw new ArgumentException(
                "OpenDisplay frames cannot be empty.",
                nameof(payload)
            );
        }

        var header = new byte[4];

        BinaryPrimitives.WriteUInt32BigEndian(
            header, 
            checked((uint)payload.Length)
        );

        await _stream.WriteAsync(
            header,
            cancellationToken
        );

        await _stream.WriteAsync(
            payload,
            cancellationToken
        );
    }

    public async Task WriteControlFrameAsync(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        if (payload.Length == 0)
        {
            throw new ArgumentException(
                "OpenDisplay control frames cannot be empty.",
                nameof(payload)
            );
        }

        if (payload.Length > MaximumControlFrameSize)
        {
            throw new ArgumentException(
                $"OpenDisplay control frames cannot exceed {MaximumControlFrameSize} bytes.",
                nameof(payload)
            );
        }

        await WriteFrameAsync(
            payload,
            cancellationToken
        );
    }
}