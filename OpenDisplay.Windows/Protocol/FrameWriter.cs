using System.Buffers.Binary;
using System.Net.Sockets;

namespace OpenDisplay.Windows.Protocol;

public class FrameWriter
{
    private readonly NetworkStream _stream;

    public FrameWriter(NetworkStream stream)
    {
        _stream = stream;
    }

    public async Task WriteFrameAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        var header = new byte[4];

        BinaryPrimitives.WriteUInt32BigEndian(header, checked((uint)payload.Length));

        await _stream.WriteAsync(header, cancellationToken);

        if (payload.Length > 0)
        {
            await _stream.WriteAsync(payload, cancellationToken);
        }
    }
}