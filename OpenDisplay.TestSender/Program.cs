using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Text.Json;
using OpenDisplay.Protocol;
using OpenDisplay.Protocol.Video;

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

var videoPath =
    Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "TestAssets",
        "test.h264"
    );

videoPath =
    Path.GetFullPath(videoPath);

Console.WriteLine();
Console.WriteLine(
    $"Lendo vídeo: {videoPath}"
);

var h264Data =
    await File.ReadAllBytesAsync(videoPath);

Console.WriteLine(
    $"Arquivo H.264: {h264Data.Length} bytes"
);

var nalUnits =
    H264AnnexBParser.Parse(h264Data);

var assembler =
    new H264AccessUnitAssembler();

var accessUnits =
    new List<H264AccessUnit>();

foreach (var nal in nalUnits)
{
    var completed =
        assembler.Add(nal);

    accessUnits.AddRange(completed);
}

var last =
    assembler.Flush();

if (last is not null)
{
    accessUnits.Add(last);
}

Console.WriteLine();
Console.WriteLine(
    $"Iniciando transmissão de {accessUnits.Count} Access Units..."
);

for (var i = 0; i < accessUnits.Count; i++)
{
    var accessUnit =
        accessUnits[i];

    var payload =
        accessUnit.ToAnnexB();

    await writer.WriteFrameAsync(
        payload,
        CancellationToken.None
    );

    Console.WriteLine(
        $"→ AU #{i} " +
        $"({payload.Length} bytes) " +
        $"keyframe={accessUnit.IsKeyFrame}"
    );

    // 30 FPS ≈ 33,33 ms por frame.
    await Task.Delay(
        TimeSpan.FromMilliseconds(33),
        CancellationToken.None
    );
}

Console.WriteLine();
Console.WriteLine("Transmissão concluída.");

Console.WriteLine();

Console.WriteLine(
    $"Access Units encontrados: {accessUnits.Count}"
);

for (
    var i = 0;
    i < Math.Min(accessUnits.Count, 10);
    i++)
{
    var accessUnit =
        accessUnits[i];

    Console.WriteLine(
        $"  AU #{i}: " +
        $"{accessUnit.NalUnits.Count} NAL(s), " +
        $"keyframe={accessUnit.IsKeyFrame}, " +
        $"size={accessUnit.ToAnnexB().Length} bytes"
    );
}

Console.WriteLine(
    $"NAL units encontrados: {nalUnits.Count}"
);

foreach (var nal in nalUnits)
{
    Console.WriteLine(
        $"  {nal.Type,-14} {nal.Data.Length,8} bytes"
    );
}

Console.WriteLine();
Console.WriteLine("Teste concluído.");