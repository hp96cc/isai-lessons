using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal static class ServerRunner
    {
        /// <summary>
        /// Poll OneDrive for new video uploads, download each valid MP4 file,
        /// run the transcription/processing pipeline, then move the remote file
        /// into a processed subfolder and remove the local copy.
        /// </summary>
        public static async Task RunAsync(OneDriveDownloader downloader)
        {
            // OneDrive folder that contains uploaded videos and the subfolder to move processed files into
            const string oneDriveFolder = "VideoUploads";
            const string processedSubFolder = "processed";

            // List files in the OneDrive "VideoUploads" folder
            var remaining = await downloader.ListFilesAsync(oneDriveFolder).ConfigureAwait(false);
            if (remaining is null || remaining.Count == 0)
            {
                Console.WriteLine($"No files found in OneDrive folder: {oneDriveFolder}");
                return;
            }

            // Ensure local download directory exists (will be used to store each MP4 temporarily)
            var downloadPath = Path.Combine(Program.BaseFolder, "Download");
            Directory.CreateDirectory(downloadPath);

            // Ensure the Whisper model is available locally; download it once if missing
            var modelPath = Program.DefaultModelPath;
            if (!File.Exists(modelPath))
            {
                await Program.DownloadModelAsync(modelPath, Program.DefaultGgmlType).ConfigureAwait(false);
            }

            // Create a single Whisper factory instance to reuse for processing all videos.
            // The factory will be disposed automatically at the end of the using block.
            using var factory = WhisperFactory.FromPath(modelPath);

            // Process files one-by-one to avoid concurrent downloads/processing and to keep behavior predictable.
            foreach (var remoteFile in remaining)
            {
                var fileNameOnly = Path.GetFileName(remoteFile);
                var ext = Path.GetExtension(fileNameOnly);

                // Only consider .mp4 files; skip any other file types
                if (!ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Skipping '{fileNameOnly}': not an .mp4");
                    continue;
                }

                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileNameOnly);

                // The filename (without extension) must be a valid integer lesson id.
                // If it isn't, skip the file so we don't accidentally process unrelated uploads.
                if (!int.TryParse(nameWithoutExt, out _))
                {
                    Console.WriteLine($"Skipping '{fileNameOnly}': filename before extension is not a valid integer");
                    continue;
                }

                Console.WriteLine($"Preparing to download and process OneDrive file: {fileNameOnly}");

                // Local path to download the mp4 to (one file at a time)
                var localMp4Path = Path.Combine(downloadPath, fileNameOnly);

                try
                {
                    // Download the remote file to the local path. DownloadFileAsync should
                    // save the file to disk; if the file is not present afterwards treat as failure.
                    var downloaded = await downloader.DownloadFileAsync(oneDriveFolder, remoteFile, localMp4Path).ConfigureAwait(false);
                    if (!File.Exists(localMp4Path))
                    {
                        // Stop processing on download failure per original behavior
                        Console.WriteLine($"Failed to download '{fileNameOnly}'. Stopping processing as requested.");
                        return;
                    }

                    Console.WriteLine($"Downloaded '{fileNameOnly}' to '{localMp4Path}'");

                    // Process the downloaded video (transcription / any other processing).
                    // Any exception here is treated as fatal and stops further processing.
                    try
                    {
                        await ProcessVideoService.ProcessVideoAndExtractSubtitlesAsync(factory, localMp4Path, Program.BaseFolder, downloader).ConfigureAwait(false);
                    }
                    catch (Exception procEx)
                    {
                        Console.WriteLine($"Processing failed for '{fileNameOnly}': {procEx.Message}. Stopping processing as requested.");
                        return;
                    }

                    // If processing succeeds, move the original remote file into the "processed" subfolder.
                    // MoveFileAsync may optionally overwrite; here it passes 'true' to allow overwrite.
                    try
                    {
                        var processedTarget = $"{oneDriveFolder}/{processedSubFolder}";
                        await downloader.MoveFileAsync(oneDriveFolder, remoteFile, processedTarget, true).ConfigureAwait(false);
                        Console.WriteLine($"Moved OneDrive file '{fileNameOnly}' to '{processedTarget}'.");
                    }
                    catch (Exception mvEx)
                    {
                        // Treat move failure as fatal to avoid re-processing the same file unexpectedly.
                        Console.WriteLine($"Failed to move OneDrive file '{fileNameOnly}' to processed folder: {mvEx.Message}. Stopping processing as requested.");
                        return;
                    }

                    // Best-effort local cleanup: delete the downloaded MP4. Failures here are ignored.
                    try
                    {
                        if (File.Exists(localMp4Path))
                            File.Delete(localMp4Path);
                    }
                    catch { /* best-effort cleanup; ignore errors */ }
                }
                catch (Exception ex)
                {
                    // Catch any unexpected errors during download/processing and stop further processing.
                    Console.WriteLine($"Unexpected error handling '{fileNameOnly}': {ex.Message}. Stopping processing as requested.");
                    return;
                }
            }
        }
    }
}