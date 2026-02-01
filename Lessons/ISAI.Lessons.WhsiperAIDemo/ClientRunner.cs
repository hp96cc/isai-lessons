using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal static class ClientRunner
    {
        public static async Task RunAsync(OneDriveDownloader downloader)
        {
            Program.InitializeLogging(Program.BaseFolder);
            Program.ConfigureFfmpeg();

            if (!Program.CheckFFmpegExists(Program.FfmpegPath))
            {
                Console.WriteLine($"FFMPeg not found at: {Program.FfmpegPath}");
                return;
            }

            var modelPath = Program.DefaultModelPath;
            if (!File.Exists(modelPath))
            {
                await Program.DownloadModelAsync(modelPath, Program.DefaultGgmlType).ConfigureAwait(false);
            }

            // Client mode: process local videos and upload results
            var mp4Files = Directory.GetFiles(Program.BaseFolder, "*.mp4", SearchOption.TopDirectoryOnly);
            if (mp4Files.Length == 0)
            {
                Console.WriteLine($"No .mp4 files found in folder: {Program.BaseFolder}");
                return;
            }

            using var factory = WhisperFactory.FromPath(modelPath);

            foreach (var videoPath in mp4Files)
            {
                try
                {
                    await Program.ProcessVideoAsync(factory, videoPath, Program.BaseFolder, downloader).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing '{videoPath}': {ex.Message}");
                }
            }
        }
    }
}