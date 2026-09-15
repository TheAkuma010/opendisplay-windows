using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace OpenDisplay.Windows.Protocol;

public class OpenDisplayServer
{
    private readonly TcpListener _listener;

    private readonly DisplayConfiguration _display;
    private readonly ReceiverIdentity _identity;

    public OpenDisplayServer(int port, DisplayConfiguration display, ReceiverIdentity identity)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _display = display;
        _identity = identity;
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
            
        }
        finally
        {
            _listener.Stop();
            Debug.WriteLine("[OpenDisplay] Server stopped.");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var connection = new OpenDisplayConnection(client, _display, _identity);

        await connection.RunAsync(cancellationToken);
    }
}