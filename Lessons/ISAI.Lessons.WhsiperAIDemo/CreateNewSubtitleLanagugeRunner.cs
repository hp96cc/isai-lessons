using FFMpegCore;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

namespace ISAI.Lessons.WhsiperAIDemo
{
    /// <summary>
    /// Downloads numeric ZIP files from OneDrive, extracts them, finds the numbered MP3 (e.g. 123.mp3),
    /// transcribes using Whisper with one or more requested languages, writes SRT named "{lessonId}__{lang}.srt",
    /// converts SRT -> VTT, uploads SRT and VTT to a configurable OneDrive folder and cleans up local files.
    /// </summary>
    internal static class CreateNewSubtitleLanagugeRunner
    {
        /// <summary>
        /// Run the subtitle generation flow for a set of language codes.
        /// - languageCodes: array of language codes (e.g. { "en", "es" }). If null or empty defaults to "en".
        /// </summary>
        public static async Task RunAsync(OneDriveDownloader downloader, string[]? languageCodes)
        {
            if (downloader == null) throw new ArgumentNullException(nameof(downloader));

            // Default to the existing automated uploads folder if configuration provides it.
            var sourceRemoteFolder = AppConfiguration.Instance.OneDrive.AutomatedProcessedUploadsFolder;
            var destinationRemoteFolder = AppConfiguration.Instance.OneDrive.SubtitlesUploadFolder; // upload back to same folder by default

            // normalize language list
            if (languageCodes == null || languageCodes.Length == 0)
            {
                Console.WriteLine("No language files provided.");
                return;
            }

            Console.WriteLine($"Listing files in OneDrive folder: {sourceRemoteFolder}");
            var files = await downloader.ListFilesAsync(sourceRemoteFolder).ConfigureAwait(false);
            if (files is null || files.Count == 0)
            {
                Console.WriteLine("No files found.");
                return;
            }

            // Only process .zip files whose filename (without extension) is numeric (e.g. 123.zip)
            var numericZips = files
                .Where(n => !string.IsNullOrWhiteSpace(n) && n.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                .Select(n => n.Trim())
                .Where(n =>
                {
                    var baseName = Path.GetFileNameWithoutExtension(n);
                    return int.TryParse(baseName, out _);
                })
                .ToList();

            if (numericZips.Count == 0)
            {
                Console.WriteLine("No numeric zip files found.");
                return;
            }

            // Ensure model present
            var modelPath = Program.DefaultModelPath;
            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"Model not found at '{modelPath}'. Downloading...");
                await Program.DownloadModelAsync(modelPath, Program.DefaultGgmlType).ConfigureAwait(false);
            }

            using var factory = WhisperFactory.FromPath(modelPath);

            foreach (var remoteZip in numericZips)
            {
                var fileNameOnly = Path.GetFileName(remoteZip);
                var lessonId = Path.GetFileNameWithoutExtension(fileNameOnly);
                Console.WriteLine($"\nProcessing remote ZIP: {fileNameOnly} (lesson id: {lessonId})");

                string? localZipPath = null;
                string extractFolder = Path.Combine(Path.GetTempPath(), "CreateNewSubtitleLanagugeRunner", Guid.NewGuid().ToString("N"));
                string? wavPath = null;

                try
                {
                    Directory.CreateDirectory(extractFolder);

                    // Download zip to temp location
                    Console.WriteLine($"Downloading '{fileNameOnly}'...");
                    localZipPath = await downloader.DownloadFileAsync(sourceRemoteFolder, fileNameOnly).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(localZipPath) || !File.Exists(localZipPath))
                    {
                        Console.WriteLine($"Download failed for '{fileNameOnly}'. Skipping.");
                        continue;
                    }

                    Console.WriteLine($"Extracting '{localZipPath}' -> '{extractFolder}'");
                    ZipFile.ExtractToDirectory(localZipPath, extractFolder, overwriteFiles: true);

                    // Look for expected mp3 (e.g. "123.mp3")
                    var expectedMp3Name = $"{lessonId}.mp3";
                    var mp3File = Directory.EnumerateFiles(extractFolder, "*.mp3", SearchOption.AllDirectories)
                        .FirstOrDefault(f => string.Equals(Path.GetFileName(f), expectedMp3Name, StringComparison.OrdinalIgnoreCase));

                    if (mp3File is null)
                    {
                        Console.WriteLine($"Expected MP3 '{expectedMp3Name}' not found in zip. Skipping.");
                        continue;
                    }

                    Console.WriteLine($"Found MP3: {mp3File}");

                    // Convert mp3 to WAV (16kHz mono PCM) required by Whisper.net
                    wavPath = await ConvertMp3ToWavAsync(mp3File, lessonId).ConfigureAwait(false);

                    // For each requested language, create a processor and transcribe separately.
                    foreach (var langCodeRaw in languageCodes)
                    {
                        if (string.IsNullOrWhiteSpace(langCodeRaw))
                            continue;

                        var langCode = langCodeRaw.Trim();
                        Console.WriteLine($"Transcribing lesson {lessonId} in language '{langCode}'");

                        using var processor = factory.CreateBuilder()
                            .WithLanguage(langCode)
                            .WithProbabilities()
                            .Build();

                        // Prepare output paths (unique per language)
                        var srtName = $"{lessonId}_{langCode}.srt";
                        var vttName = $"{lessonId}_{langCode}.vtt";
                        var srtPath = Path.Combine(extractFolder, srtName);
                        var vttPath = Path.Combine(extractFolder, vttName);

                        // Transcribe and write SRT and VTT (incremental)
                        await TranscribeAndWriteSubtitlesAsync(processor, wavPath, srtPath, vttPath).ConfigureAwait(false);

                        // Ensure VTT exists (convert if necessary)
                        if (!File.Exists(vttPath) && File.Exists(srtPath))
                        {
                            ConvertSrtToVtt(srtPath, vttPath);
                        }

                        // Upload SRT and VTT
                        Console.WriteLine($"Uploading {srtName} to OneDrive folder: {destinationRemoteFolder}");
                        var uploadedSrt = await downloader.UploadFileToOneDriveAsync(srtPath, destinationRemoteFolder).ConfigureAwait(false);
                        Console.WriteLine($"SRT upload success: {uploadedSrt}");

                        Console.WriteLine($"Uploading {vttName} to OneDrive folder: {destinationRemoteFolder}");
                        var uploadedVtt = await downloader.UploadFileToOneDriveAsync(vttPath, destinationRemoteFolder).ConfigureAwait(false);
                        Console.WriteLine($"VTT upload success: {uploadedVtt}");
                    }

                    // Optionally: move remote zip to processed location, or leave as-is. Not implemented by default.
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing '{fileNameOnly}': {ex.Message}");
                }
                finally
                {
                    // Cleanup local zip
                    try
                    {
                        if (!string.IsNullOrEmpty(localZipPath) && File.Exists(localZipPath))
                            File.Delete(localZipPath);
                    }
                    catch { /* best-effort */ }

                    // Cleanup temporary wav (only here, after all languages done)
                    try
                    {
                        if (!string.IsNullOrEmpty(wavPath) && File.Exists(wavPath))
                            File.Delete(wavPath);
                    }
                    catch { /* best-effort */ }

                    // Cleanup extracted folder
                    try
                    {
                        if (Directory.Exists(extractFolder))
                            Directory.Delete(extractFolder, recursive: true);
                    }
                    catch { /* best-effort */ }
                }
            }
        }

        // Convert MP3 to 16kHz mono WAV; returns wav path
        private static async Task<string> ConvertMp3ToWavAsync(string mp3Path, string nameWithoutExt)
        {
            Console.WriteLine("Converting MP3 to WAV (16kHz mono)");
            var tempWavPath = Path.Combine(Path.GetTempPath(), $"{nameWithoutExt}_{Guid.NewGuid():N}.wav");

            try
            {
                await FFMpegArguments
                    .FromFileInput(mp3Path)
                    .OutputToFile(tempWavPath, overwrite: true, options => options
                        .WithAudioCodec("pcm_s16le")
                        .WithAudioSamplingRate(16000)
                        .WithCustomArgument("-ac 1"))
                    .ProcessAsynchronously()
                    .ConfigureAwait(false);

                Console.WriteLine($"WAV conversion completed: {tempWavPath}");
                return tempWavPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to convert MP3 to WAV: {ex.Message}");
                throw;
            }
        }

        // Streams wav into Whisper processor and writes SRT + VTT incrementally (does NOT delete wav)
        private static async Task TranscribeAndWriteSubtitlesAsync(WhisperProcessor processor, string wavPath, string srtPath, string vttPath)
        {
            await using var wavStream = File.OpenRead(wavPath);
            await using var srtWriter = new StreamWriter(srtPath, false, Encoding.UTF8);
            await using var vttWriter = new StreamWriter(vttPath, false, Encoding.UTF8);

            // VTT header
            await vttWriter.WriteLineAsync("WEBVTT").ConfigureAwait(false);
            await vttWriter.WriteLineAsync().ConfigureAwait(false);

            var index = 1;
            await foreach (var segment in processor.ProcessAsync(wavStream))
            {
                // SRT
                await srtWriter.WriteLineAsync(index.ToString()).ConfigureAwait(false);
                await srtWriter.WriteLineAsync($"{FormatSrtTimestamp(segment.Start)} --> {FormatSrtTimestamp(segment.End)}").ConfigureAwait(false);
                await srtWriter.WriteLineAsync(segment.Text.Trim()).ConfigureAwait(false);
                await srtWriter.WriteLineAsync().ConfigureAwait(false);

                // VTT (dot milliseconds)
                await vttWriter.WriteLineAsync($"{FormatVttTimestamp(segment.Start)} --> {FormatVttTimestamp(segment.End)}").ConfigureAwait(false);
                await vttWriter.WriteLineAsync(segment.Text.Trim()).ConfigureAwait(false);
                await vttWriter.WriteLineAsync().ConfigureAwait(false);

                index++;
            }
        }

        // Convert an existing SRT to VTT (simple conversion of timestamp format and stripping index lines)
        private static void ConvertSrtToVtt(string srtPath, string vttPath)
        {
            try
            {
                using var reader = new StreamReader(srtPath, Encoding.UTF8);
                using var writer = new StreamWriter(vttPath, false, Encoding.UTF8);

                writer.WriteLine("WEBVTT");
                writer.WriteLine();

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    // skip numeric index lines
                    if (int.TryParse(line.Trim(), out _))
                    {
                        continue;
                    }

                    // timestamp line: replace comma with dot for milliseconds
                    if (line.Contains("-->"))
                    {
                        var converted = line.Replace(',', '.');
                        writer.WriteLine(converted);
                        continue;
                    }

                    writer.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to convert SRT to VTT: {ex.Message}");
            }
        }

        // Format a TimeSpan for SRT (comma milliseconds)
        private static string FormatSrtTimestamp(TimeSpan time)
        {
            return $"{time:hh\\:mm\\:ss},{time.Milliseconds:D3}";
        }

        // Format a TimeSpan for VTT (dot milliseconds)
        private static string FormatVttTimestamp(TimeSpan time)
        {
            return $"{time:hh\\:mm\\:ss}.{time.Milliseconds:D3}";
        }
    }
}