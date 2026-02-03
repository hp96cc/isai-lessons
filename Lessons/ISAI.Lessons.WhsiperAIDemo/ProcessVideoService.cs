using FFMpegCore;
using Microsoft.Identity.Client;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Whisper.net;

namespace ISAI.Lessons.WhsiperAIDemo
{
    /// <summary>
    /// Service that encapsulates video processing and subtitle extraction logic.
    /// This contains the former Program.ProcessVideoAsync and its closely related helpers.
    /// </summary>
    internal static class ProcessVideoService
    {
        /// <summary>
        /// Processes a video: creates thumbnail & HLS (m3u8), extracts audio, transcribes to SRT/VTT,
        /// zips results and uploads via provided downloader.
        /// </summary>
        public static async Task ProcessVideoAndExtractSubtitlesAsync(WhisperFactory factory, string videoPath, string baseFolder, OneDriveDownloader downloader)
        {
            var fileName = Path.GetFileName(videoPath);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            Console.WriteLine($"\nProcessing: {fileName}");

            // Create expected output folder structure
            var outputFolder = Path.Combine(baseFolder, nameWithoutExt);
            var outputFolderM3U8 = Path.Combine(baseFolder, outputFolder, "m3u8");
            var outputFolderM3U8Signed = Path.Combine(baseFolder, outputFolder, "m3u8-signed");

            // Ensure a fresh output folder for this video
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);

            Directory.CreateDirectory(outputFolder);
            Directory.CreateDirectory(outputFolderM3U8);
            Directory.CreateDirectory(outputFolderM3U8Signed);

            // Paths for artifacts produced during processing
            var mp4Path = Path.Combine(outputFolder, nameWithoutExt + ".mp4");
            var mp3Path = Path.Combine(outputFolder, nameWithoutExt + ".mp3");
            var srtPath = Path.Combine(outputFolder, nameWithoutExt + ".srt");
            var vttPath = Path.Combine(outputFolder, nameWithoutExt + ".vtt");
            var thumbImage = Path.Combine(outputFolderM3U8, nameWithoutExt + ".jpg");
            var m3u8File = Path.Combine(outputFolderM3U8, nameWithoutExt + ".m3u8");

            // Copy main file into output for transport / zipping
            File.Copy(videoPath, mp4Path);

            // FFmpeg command args for thumbnail and HLS output.
            // Note: keep arguments quoted to support paths with spaces.
            var encodingArgsThumb = $"-i \"{videoPath}\" -ss 00:00:00 -frames:v 1 \"{thumbImage}\"";
            var encodingArgsM3U8 = $"-i \"{videoPath}\" -vcodec h264 -b:a 96k -start_number 0 -hls_time 10 -hls_list_size 0 -f hls -hls_enc 1 \"{m3u8File}\"";

            // Execute ffmpeg for thumbnail and HLS; bail on failure
            var result = await EncodeFFMpeg(encodingArgsThumb).ConfigureAwait(false);
            if (result)
            {
                result = await EncodeFFMpeg(encodingArgsM3U8).ConfigureAwait(false);
            }

            if (!result)
            {
                Console.WriteLine("FFMpeg Encoiding failed");
                return;
            }

            // Ensure audio is available for transcription (extract if necessary)
            EnsureAudioExtracted(videoPath, mp3Path);

            // Create a processor per file (Whisper) and dispose promptly
            using var processor = factory.CreateBuilder()
                .WithLanguage("en")
                .WithProbabilities()
                .Build();

            string tempWavPath = null;
            try
            {
                // Convert MP3 to a temporary WAV file (16 kHz) for Whisper input
                tempWavPath = ConvertMp3ToWav(mp3Path, nameWithoutExt);

                // Transcribe and write SRT/VTT incrementally to avoid memory pressure
                await TranscribeAndWriteSubtitlesAsync(processor, tempWavPath, srtPath, vttPath).ConfigureAwait(false);

                Console.WriteLine($"\nWebVTT file created: {vttPath}");

                var destDir = Path.Combine(baseFolder, nameWithoutExt);
                Directory.CreateDirectory(destDir);

                var destVideoPath = Path.Combine(destDir, Path.GetFileName(videoPath));

                try
                {
                    // Create a zip for upload
                    var zipPath = CreateZipForFolder(destDir);
                    if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
                    {
                        Console.WriteLine($"Created zip: {zipPath}");

                        // Best-effort: remove source folder after zip created
                        try
                        {
                            if (Directory.Exists(destDir))
                            {
                                Directory.Delete(destDir, recursive: true);
                                Console.WriteLine($"Deleted source folder: {destDir}");
                            }
                        }
                        catch (Exception delEx)
                        {
                            Console.WriteLine($"Failed to delete folder '{destDir}': {delEx.Message}");
                        }

                        try
                        {
                            // Upload to OneDrive and on success move zip to local 'complete' folder
                            var uploaded = await downloader.UploadFileToOneDriveAsync(zipPath, "AutomatedUploads").ConfigureAwait(false);
                            if (uploaded)
                            {
                                Console.WriteLine("Upload to OneDrive succeeded.");

                                try
                                {
                                    var completeDir = Path.Combine(baseFolder, "complete");
                                    Directory.CreateDirectory(completeDir);

                                    var destZipPath = Path.Combine(completeDir, Path.GetFileName(zipPath));
                                    if (File.Exists(destZipPath))
                                    {
                                        File.Delete(destZipPath);
                                    }

                                    File.Move(zipPath, destZipPath);
                                    Console.WriteLine($"Moved ZIP to complete folder: {destZipPath}");

                                    // Remove original video to avoid re-processing
                                    File.Delete(videoPath);
                                }
                                catch (Exception moveEx)
                                {
                                    Console.WriteLine($"Failed to move ZIP to complete folder: {moveEx.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Upload to OneDrive failed (returned false).");
                            }
                        }
                        catch (MsalException mex)
                        {
                            Console.WriteLine($"OneDrive authentication error: {mex.Message}");
                        }
                        catch (HttpRequestException hex)
                        {
                            Console.WriteLine($"OneDrive upload network error: {hex.Message}");
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("OneDrive upload was cancelled.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error uploading to OneDrive: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Zip creation failed for folder: {destDir}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error zipping/uploading '{destDir}': {ex.Message}");
                }

            }
            finally
            {
                // Always attempt to clean up the temporary WAV file (best-effort)
                if (!string.IsNullOrEmpty(tempWavPath) && File.Exists(tempWavPath))
                {
                    try { File.Delete(tempWavPath); }
                    catch { /* best-effort cleanup */ }
                }
            }
        }

        // Ensures mp3 audio file exists; if not, extracts from source video via FFMpeg.
        private static void EnsureAudioExtracted(string videoPath, string mp3Path)
        {
            if (!File.Exists(mp3Path))
            {
                Console.WriteLine($"Extracting audio to: {mp3Path}");
                FFMpeg.ExtractAudio(videoPath, mp3Path);
            }
            else
            {
                Console.WriteLine($"Using existing audio: {mp3Path}");
            }
        }

        // Converts MP3 to a 16kHz WAV file on disk using NAudio resampling.
        // Returns path to the temporary WAV created.
        private static string ConvertMp3ToWav(string mp3Path, string nameWithoutExt)
        {
            Console.WriteLine("Converting to WAV (temporary file)");
            var tempWavPath = Path.Combine(Path.GetTempPath(), $"{nameWithoutExt}_{Guid.NewGuid():N}.wav");

            using var fileStream = File.OpenRead(mp3Path);
            using var reader = new Mp3FileReader(fileStream);
            var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);

            // Create 16-bit PCM WAV file expected by Whisper processors
            WaveFileWriter.CreateWaveFile16(tempWavPath, resampler);

            return tempWavPath;
        }

        // Streams the WAV into the Whisper processor and writes SRT + VTT incrementally.
        private static async Task TranscribeAndWriteSubtitlesAsync(WhisperProcessor processor, string wavPath, string srtPath, string vttPath)
        {
            using var wavStream = File.OpenRead(wavPath);

            // Open both writers and ensure UTF-8 output (SRT/VTT are text formats)
            await using var srtWriter = new StreamWriter(srtPath, false, Encoding.UTF8);
            await using var vttWriter = new StreamWriter(vttPath, false, Encoding.UTF8);

            // VTT requires a header
            await vttWriter.WriteLineAsync("WEBVTT").ConfigureAwait(false);
            await vttWriter.WriteLineAsync().ConfigureAwait(false);

            var index = 1;
            // Whisper processor yields segments asynchronously; write as they arrive
            await foreach (var segment in processor.ProcessAsync(wavStream))
            {
                // SRT format: numeric index + timestamp with comma for milliseconds
                await srtWriter.WriteLineAsync(index.ToString()).ConfigureAwait(false);
                await srtWriter.WriteLineAsync($"{FormatSrtTimestamp(segment.Start)} --> {FormatSrtTimestamp(segment.End)}").ConfigureAwait(false);
                await srtWriter.WriteLineAsync(segment.Text.Trim()).ConfigureAwait(false);
                await srtWriter.WriteLineAsync().ConfigureAwait(false);

                // VTT format: use dot for milliseconds and no numeric index required
                await vttWriter.WriteLineAsync($"{FormatVttTimestamp(segment.Start)} --> {FormatVttTimestamp(segment.End)}").ConfigureAwait(false);
                await vttWriter.WriteLineAsync(segment.Text.Trim()).ConfigureAwait(false);
                await vttWriter.WriteLineAsync().ConfigureAwait(false);

                index++;
            }
        }

        // Format a TimeSpan for SRT (uses comma as millisecond separator)
        private static string FormatSrtTimestamp(TimeSpan time)
        {
            return $"{time:hh\\:mm\\:ss},{time.Milliseconds:D3}";
        }

        // Format a TimeSpan for VTT (uses period as millisecond separator)
        private static string FormatVttTimestamp(TimeSpan time)
        {
            return $"{time:hh\\:mm\\:ss}.{time.Milliseconds:D3}";
        }

        /// <summary>
        /// Creates a zip file for the given folder. Returns the zip path or null on failure.
        /// </summary>
        private static string CreateZipForFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"Folder not found for zipping: {folderPath}");
                    return null;
                }

                var parent = Path.GetDirectoryName(folderPath) ?? folderPath;
                var zipName = Path.GetFileName(folderPath) + ".zip";
                var zipPath = Path.Combine(parent, zipName);

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                ZipFile.CreateFromDirectory(folderPath, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
                return zipPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create zip for '{folderPath}': {ex.Message}");
                return null;
            }
        }

        // Wrapper to call ffmpeg and report success as boolean
        private static async Task<bool> EncodeFFMpeg(string encodingArgs)
        {
            int exitCode = await ExecuteFFmpegAsync(Program.FfmpegPath, encodingArgs).ConfigureAwait(false);

            if (exitCode == 0)
            {
                Console.WriteLine("Encoding completed successfully!");
                return true;
            }
            else
            {
                Console.WriteLine($"Encoding failed with exit code: {exitCode}");
                return false;
            }
        }

        // Executes ffmpeg as a child process and streams stdout/stderr to console in real time.
        private static async Task<int> ExecuteFFmpegAsync(string ffmpegPath, string arguments)
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using (var process = System.Diagnostics.Process.Start(processInfo))
            {
                // Read output in real-time and forward to console
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine(e.Data);
                };

                // Read error/progress in real-time
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync().ConfigureAwait(false);

                return process.ExitCode;
            }
        }
    }
}