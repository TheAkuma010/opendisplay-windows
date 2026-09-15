using System.Buffers.Binary;
using System.Net.Sockets;
using System.IO;

namespace OpenDisplay.Protocol;

public class FrameReader
{
    private const uint MaximumFrameSize = 64 * 1024 * 1024;
    private readonly NetworkStream _stream;

    public FrameReader(NetworkStream stream)
    {
        _stream = stream;
    }

    public async Task<byte[]> ReadFrameAsync(CancellationToken cancellationToken)
    {
        var header = new byte[4];

        await ReadExactlyAsync(header, cancellationToken);

        var payloadLenght = BinaryPrimitives.ReadUInt32BigEndian(header);

        if (payloadLenght == 0)
        {
            throw new InvalidDataException("Frame payload length cannot be zero.");
        }

        if (payloadLenght > MaximumFrameSize)
        {
            throw new InvalidDataException($"Frame payload length exceeds maximum allowed size of {MaximumFrameSize} bytes.");
        }

        var payload = new byte[(int)payloadLenght];

        await ReadExactlyAsync(payload, cancellationToken);

        return payload;
    }

    private async Task ReadExactlyAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var offset = 0;

        while (offset < buffer.Length)
        {
            var bytesRead = await _stream.ReadAsync(
                buffer[offset..],
                cancellationToken
            );

            if (bytesRead == 0)
            {
                throw new EndOfStreamException("Unexpected end of stream.");
            }

            offset += bytesRead;
        }
    }
}