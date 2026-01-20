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
            const string oneDriveFolder = "VideoUploads";
            const string processedSubFolder = "processed";

            var remaining = await downloader.ListFilesAsync(oneDriveFolder).ConfigureAwait(false);
            if (remaining is null || remaining.Count == 0)
            {
                Console.WriteLine($"No files found in OneDrive folder: {oneDriveFolder}");
                return;
            }

            // Ensure local download dir exists
            var downloadPath = Path.Combine(Program.BaseFolder, "Download");
            Directory.CreateDirectory(downloadPath);

            // Load model once
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

                // Only consider .mp4 files
                if (!ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Skipping '{fileNameOnly}': not an .mp4");
                    continue;
                }

                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileNameOnly);

                // Name must be a valid integer lesson id
                if (!int.TryParse(nameWithoutExt, out _))
                {
                    Console.WriteLine($"Skipping '{fileNameOnly}': filename before extension is not a valid integer");
                    continue;
                }

                Console.WriteLine($"Preparing to download and process OneDrive file: {fileNameOnly}");

                // Download the single mp4 to a local path (one-at-a-time)
                var localMp4Path = Path.Combine(downloadPath, fileNameOnly);

                try
                {
                    var downloaded = await downloader.DownloadFileAsync(oneDriveFolder, remoteFile, localMp4Path).ConfigureAwait(false);
                    if (!File.Exists(localMp4Path))
                    {
                        Console.WriteLine($"Failed to download '{fileNameOnly}'. Stopping processing as requested.");
                        return; // stop on failure
                    }

                    Console.WriteLine($"Downloaded '{fileNameOnly}' to '{localMp4Path}'");

                    // Process the downloaded video file
                    try
                    {
                        await Program.ProcessVideoAsync(factory, localMp4Path, Program.BaseFolder, downloader).ConfigureAwait(false);
                    }
                    catch (Exception procEx)
                    {
                        Console.WriteLine($"Processing failed for '{fileNameOnly}': {procEx.Message}. Stopping processing as requested.");
                        return; // stop on failure
                    }

                    // On success, move remote file to processed subfolder
                    try
                    {
                        var processedTarget = $"{oneDriveFolder}/{processedSubFolder}";
                        await downloader.MoveFileAsync(oneDriveFolder, remoteFile, processedTarget, true).ConfigureAwait(false);
                        Console.WriteLine($"Moved OneDrive file '{fileNameOnly}' to '{processedTarget}'.");
                    }
                    catch (Exception mvEx)
                    {
                        Console.WriteLine($"Failed to move OneDrive file '{fileNameOnly}' to processed folder: {mvEx.Message}. Stopping processing as requested.");
                        return; // stop on failure
                    }

                    // Clean up local copy (best-effort)
                    try
                    {
                        if (File.Exists(localMp4Path))
                            File.Delete(localMp4Path);
                    }
                    catch { /* best-effort cleanup */ }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error handling '{fileNameOnly}': {ex.Message}. Stopping processing as requested.");
                    return; // stop on failure
                }
            }
        }
    }
}