using System;
using System.IO;
using FFmpeg.AutoGen;

namespace OpenDisplay.Windows.Video;

public static class FFmpegLoader
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        var ffmpegPath = Path.Combine(
                AppContext.BaseDirectory,
                "FFmpeg"
            );

        if (!Directory.Exists(ffmpegPath))
        {
            throw new DirectoryNotFoundException(
                $"Diretório do FFmpeg não encontrado: {ffmpegPath}"
            );
        }

        ffmpeg.RootPath = ffmpegPath;

        _initialized = true;
    }
}