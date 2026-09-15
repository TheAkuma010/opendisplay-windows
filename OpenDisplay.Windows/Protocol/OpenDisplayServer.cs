using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace OpenDisplay.Windows.Protocol;

public class OpenDisplayServer
{
    private readonly TcpListener _listener;

    public OpenDisplayServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();

        Debug.WriteLine(
            $"[OpenDisplay] Listening on port {_listener.LocalEndpoint} for incoming connections..."
        );

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);

                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        finally
        {
            _listener.Stop();
            Debug.WriteLine("[OpenDisplay] Server stopped.");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var connection = new OpenDisplayConnection(client);

        await connection.RunAsync(cancellationToken);
    }
}