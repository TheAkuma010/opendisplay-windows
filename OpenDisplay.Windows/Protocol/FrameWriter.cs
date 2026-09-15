using System.Buffers.Binary;
using System.Net.Sockets;

namespace OpenDisplay.Windows.Protocol;

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

        if (payload.Length > MaximumControlFrameSize)
        {
            throw new ArgumentException(
                $"Frame exceeds the OpenDisplay control frame limit.",
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
}