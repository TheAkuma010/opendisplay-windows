using System.Net.Sockets;
using System.Diagnostics;

namespace OpenDisplay.Windows.Protocol;

public class OpenDisplayConnection
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;

    private readonly FrameReader _frameReader;
    private readonly FrameWriter _frameWriter;

    public OpenDisplayConnection(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();

        _frameReader = new FrameReader(_stream);
        _frameWriter = new FrameWriter(_stream);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var remoteEndPoint = _client.Client.RemoteEndPoint;

        Debug.WriteLine(
            $"[OpenDisplay] Connection started: {remoteEndPoint}"
        );

        try
        {
            while (!cancellationToklen.IsCancellationRequested)
            {
                var frame = await _frameReader.ReadFrameAsync(cancellationToken);

                Debug.WriteLine(
                    $"[OpenDisplay] Frame received: {frame.Type} with {frame.bytes} bytes from {remoteEndPoint}"
                );
            }
        }
        catch (EndOfStreamException)
        {
            Debug.WriteLine(
                $"[OpenDisplay] Connection close by client: {remoteEndPoint}"
            );
        }
        catch (OperationCanceledException)
        {
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[OpenDisplay] Connection error: {ex.Message} from {remoteEndPoint}"
            );
        }
        finally
        {
            _client.Close();

            Debug.WriteLine(
                $"[OpenDisplay] Connection ended: {remoteEndPoint}"
            );
        }
    }
}