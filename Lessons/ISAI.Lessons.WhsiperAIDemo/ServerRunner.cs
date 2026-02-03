using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal static class ServerRunner
    {
        public static async Task RunAsync(OneDriveDownloader downloader)
        {
            var config = AppConfiguration.Instance.OneDrive;
            
            var remaining = await downloader.ListFilesAsync(config.VideoUploadsFolder).ConfigureAwait(false);
            if (remaining is null || remaining.Count == 0)
            {
                Console.WriteLine($"No files found in OneDrive folder: {config.VideoUploadsFolder}");
                return;
            }

            var downloadPath = Path.Combine(Program.BaseFolder, "Download");
            Directory.CreateDirectory(downloadPath);

            var modelPath = Program.DefaultModelPath;
            if (!File.Exists(modelPath))
            {
                await Program.DownloadModelAsync(modelPath, Program.DefaultGgmlType).ConfigureAwait(false);
            }

            using var factory = WhisperFactory.FromPath(modelPath);

            foreach (var remoteFile in remaining)
            {
                var fileNameOnly = Path.GetFileName(remoteFile);
                var ext = Path.GetExtension(fileNameOnly);

                if (!ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Skipping '{fileNameOnly}': not an .mp4");
                    continue;
                }

                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileNameOnly);

                if (!int.TryParse(nameWithoutExt, out _))
                {
                    Console.WriteLine($"Skipping '{fileNameOnly}': filename before extension is not a valid integer");
                    continue;
                }

                Console.WriteLine($"Preparing to download and process OneDrive file: {fileNameOnly}");

                var localMp4Path = Path.Combine(downloadPath, fileNameOnly);

                try
                {
                    var downloaded = await downloader.DownloadFileAsync(config.VideoUploadsFolder, remoteFile, localMp4Path).ConfigureAwait(false);
                    if (!File.Exists(localMp4Path))
                    {
                        Console.WriteLine($"Failed to download '{fileNameOnly}'. Stopping processing as requested.");
                        return;
                    }

                    Console.WriteLine($"Downloaded '{fileNameOnly}' to '{localMp4Path}'");

                    try
                    {
                        await ProcessVideoService.ProcessVideoAndExtractSubtitlesAsync(factory, localMp4Path, Program.BaseFolder, downloader).ConfigureAwait(false);
                    }
                    catch (Exception procEx)
                    {
                        Console.WriteLine($"Processing failed for '{fileNameOnly}': {procEx.Message}. Stopping processing as requested.");
                        return;
                    }

                    try
                    {
                        var processedTarget = $"{config.VideoUploadsFolder}/{config.ProcessedSubFolder}";
                        await downloader.MoveFileAsync(config.VideoUploadsFolder, remoteFile, processedTarget, true).ConfigureAwait(false);
                        Console.WriteLine($"Moved OneDrive file '{fileNameOnly}' to '{processedTarget}'.");
                    }
                    catch (Exception mvEx)
                    {
                        Console.WriteLine($"Failed to move OneDrive file '{fileNameOnly}' to processed folder: {mvEx.Message}. Stopping processing as requested.");
                        return;
                    }

                    try
                    {
                        if (File.Exists(localMp4Path))
                            File.Delete(localMp4Path);
                    }
                    catch { /* best-effort cleanup; ignore errors */ }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error handling '{fileNameOnly}': {ex.Message}. Stopping processing as requested.");
                    return;
                }
            }
        }
    }
}