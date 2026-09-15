using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using OpenDisplay.Protocol;
using OpenDisplay.Protocol.Video;

namespace OpenDisplay.Windows.Protocol;

public class OpenDisplayConnection
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;

    private readonly FrameReader _frameReader;
    private readonly FrameWriter _frameWriter;

    private readonly DisplayConfiguration _display;
    private readonly ReceiverIdentity _identity;

    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public OpenDisplayConnection(TcpClient client, DisplayConfiguration display, ReceiverIdentity identity)
    {
        _client = client;
        _display = display;
        _identity = identity;

        _client.NoDelay = true;

        _stream = _client.GetStream();

        _frameReader = new FrameReader(_stream);
        _frameWriter = new FrameWriter(_stream);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var remoteEndPoint = _client.Client.RemoteEndPoint;

        Console.WriteLine(
            $"[OpenDisplay] Connection started: {remoteEndPoint}"
        );

        try
        {
            await SendHelloAsync(cancellationToken);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var pingTask = RunPingLoopAsync(linkedCts.Token);

            try
            {
                await ReceiveLoopAsync(linkedCts.Token);
            }
            finally
            {
                linkedCts.Cancel();

                try
                {
                    await pingTask;
                }
                catch (OperationCanceledException)
                {
                }
            }

        }
        catch (EndOfStreamException)
        {
            Console.WriteLine(
                $"[OpenDisplay] Connection close by client: {remoteEndPoint}"
            );
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[OpenDisplay] Connection error: {ex.Message} from {remoteEndPoint}"
            );
        }
        finally
        {
            _client.Close();

            Console.WriteLine(
                $"[OpenDisplay] Connection ended: {remoteEndPoint}"
            );
        }
    }

    private async Task SendHelloAsync(CancellationToken cancellationToken)
    {
        var message = new HelloMessage
        {
            PixelsWide = _display.PixelsWide,
            PixelsHigh = _display.PixelsHigh,
            Scale = _display.Scale,
            Device = _display.Device,
            Id = _identity.Id,
            ProtocolVersion = _display.ProtocolVersion,
        };

        // var payload = MessageSerializer.Serialize(message);

        // await _frameWriter.WriteFrameAsync(payload, cancellationToken);

        await SendControlMessageAsync(message, cancellationToken);

        Console.WriteLine(
            "[OpenDisplay] → hello"
        );

        Console.WriteLine(
            $"[OpenDisplay]    " +
            $"{_display.PixelsWide} x {_display.PixelsHigh}"
        );

        Console.WriteLine(
            $"[OpenDisplay]    " +
            $"scale={_display.Scale}"
        );

        Console.WriteLine(
            $"[OpenDisplay]    " +
            $"pv={_display.ProtocolVersion}"
        );

        Console.WriteLine(
            $"[OpenDisplay]    " +
            $"id={_identity.Id}"
        );
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var frame = await _frameReader.ReadFrameAsync(cancellationToken);

            if (MessageParser.IsJson(frame))
            {
                await HandleJsonMessageAsync(frame, cancellationToken);
            }
            else
            {
                HandleVideoFrame(frame);
            }
        }
    }

    private async Task HandleJsonMessageAsync(byte[] frame, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(frame);

            var root = document.RootElement;

            if (!root.TryGetProperty("type", out var typeProperty))
            {
                Console.WriteLine(
                    "[OpenDisplay] Ignoring JSON without type."
                );

                return;
            }

            var type = typeProperty.GetString();

            switch (type)
            {
                case "welcome":
                    HandleWelcome(root);
                    break;
                case "ping":
                    await HandlePingAsync(
                        root,
                        cancellationToken
                    );
                    break;
                case "cursor":
                    Console.WriteLine(
                        "[OpenDisplay] ← cursor"
                    );
                    break;
                case "cursorImg":
                    Console.WriteLine(
                        "[OpenDisplay] ← cursorImg"
                    );
                    break;
                case "updateRequired":
                    Console.WriteLine(
                        "[OpenDisplay] ← updateRequired"
                    );
                    break;
                default:
                    Console.WriteLine(
                        $"[OpenDisplay] Ignoring unknown " +
                        $"message type: {type}"
                    );
                    break;
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine(
                $"[OpenDisplay] Invalid JSON: {ex.Message}"
            );
        }
    }

    private void HandleWelcome(JsonElement root)
    {
        var pv =
            root.TryGetProperty(
                "pv",
                out var pvProperty)
                ? pvProperty.GetInt32()
                : 1;

        var minimum =
            root.TryGetProperty(
                "min",
                out var minProperty)
                ? minProperty.GetInt32()
                : 1;

        Console.WriteLine(
            $"[OpenDisplay] ← welcome: " +
            $"pv={pv}, min={minimum}"
        );

        if (pv < _display.ProtocolVersion)
        {
            Console.WriteLine(
                $"[OpenDisplay] Warning: sender uses " +
                $"an older protocol version."
            );
        }

        if (_display.ProtocolVersion < minimum)
        {
            Console.WriteLine(
                $"[OpenDisplay] Warning: sender requires " +
                $"a newer protocol version."
            );
        }
    }

    private async Task HandlePingAsync(JsonElement root, CancellationToken cancellationToken)
    {
        if (!root.TryGetProperty("t", out var timestampProperty))
        {
            return;
        }

        var timestamp = timestampProperty.GetInt64();

        var response = new PongMessage
        {
            Timestamp = timestamp,
            SenderTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        // var payload = MessageSerializer.Serialize(response);

        await SendControlMessageAsync(response, cancellationToken);

        Console.WriteLine(
            "[OpenDisplay] → pong"
        );
    }

    private async Task RunPingLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            var message = new PingMessage
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // var payload = MessageSerializer.Serialize(message);

            await SendControlMessageAsync(message, cancellationToken);

            Console.WriteLine(
                "[OpenDisplay] → ping"
            );
        }
    }

    private void HandleVideoFrame(byte[] frame)
    {
        var accessUnit =
            H264AnnexBParser.Parse(frame);

        Console.WriteLine(
            $"[OpenDisplay] ← video frame " +
            $"({frame.Length} bytes)"
        );

        Console.WriteLine(
            $"[OpenDisplay]    NALs: {accessUnit.Count}"
        );

        foreach (var nal in accessUnit)
        {
            Console.WriteLine(
                $"[OpenDisplay]    {nal.Type} " +
                $"({nal.Data.Length} bytes)"
            );
        }
    }

    private async Task SendControlMessageAsync<T>(T message, CancellationToken cancellationToken)
    {
        var payload = MessageSerializer.Serialize(message);

        await _writeLock.WaitAsync(cancellationToken);

        try
        {
            await _frameWriter.WriteControlFrameAsync(payload, cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}