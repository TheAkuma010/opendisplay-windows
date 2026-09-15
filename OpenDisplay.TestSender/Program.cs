using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Text.Json;
using OpenDisplay.Protocol;

Console.WriteLine("==============================");
Console.WriteLine("=  OpenDisplay Test Sender   =");
Console.WriteLine("==============================");

using var client = new TcpClient();

Console.WriteLine("Conectando ao receiver...");

await client.ConnectAsync(
    "127.0.0.1",
    9000
);

client.NoDelay = true;

Console.WriteLine("Conectado.");

using var stream = client.GetStream();

var reader = new FrameReader(stream);
var writer = new FrameWriter(stream);

Console.WriteLine("Aguardando hello...");

var helloFrame = await reader.ReadFrameAsync(CancellationToken.None);

if (!MessageParser.IsJson(helloFrame))
{
    throw new InvalidDataException("O primeiro frame recebido nãe é JSON.");
}

using var helloDocument = JsonDocument.Parse(helloFrame);

var hello = helloDocument.RootElement;

Console.WriteLine("← hello");

Console.WriteLine(
    $"   pixelsWide: {hello.GetProperty("pixelsWide").GetInt32()}"
);

Console.WriteLine(
    $"   pixelsHigh: {hello.GetProperty("pixelsHigh").GetInt32()}"
);

Console.WriteLine(
    $"   scale: {hello.GetProperty("scale").GetDouble()}"
);

Console.WriteLine(
    $"   device: {hello.GetProperty("device").GetString()}"
);

Console.WriteLine(
    $"   pv: {hello.GetProperty("pv").GetInt32()}"
);

Console.WriteLine(
    $"   id: {hello.GetProperty("id").GetString()}"
);

var welcome = new WelcomeMessage
{
    ProtocolVersion = 3,
    MinimumProtocolVersion = 1
};

await writer.WriteControlFrameAsync(
    MessageSerializer.Serialize(welcome),
    CancellationToken.None
);

Console.WriteLine("→ welcome");

var ping = new PingMessage
{
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

await writer.WriteControlFrameAsync(
    MessageSerializer.Serialize(ping),
    CancellationToken.None
);

Console.WriteLine("→ ping");

var pongFrame =
    await reader.ReadFrameAsync(
        CancellationToken.None
    );

if (!MessageParser.IsJson(pongFrame))
{
    throw new InvalidDataException(
        "Resposta ao ping não é JSON."
    );
}

using var pongDocument =
    JsonDocument.Parse(pongFrame);

var pong =
    pongDocument.RootElement;

Console.WriteLine(
    $"← {pong.GetProperty("type").GetString()}"
);

Console.WriteLine(
    $"   t: {pong.GetProperty("t").GetInt64()}"
);

Console.WriteLine(
    $"   mt: {pong.GetProperty("mt").GetInt64()}"
);

var fakeVideo = new byte[]
{
    0x00, 0x00, 0x00, 0x01,
    0x67, 0x42, 0x00, 0x1F,
    0x00, 0x00, 0x00, 0x01,
    0x68, 0xCE, 0x3C, 0x80
};

await writer.WriteFrameAsync(
    fakeVideo,
    CancellationToken.None
);

Console.WriteLine(
    $"→ fake video ({fakeVideo.Length} bytes)"
);

Console.WriteLine();
Console.WriteLine("Teste concluído.");