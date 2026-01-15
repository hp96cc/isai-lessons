using FFMpegCore;
using Microsoft.Extensions.AI;
using Microsoft.Identity.Client;    
using Microsoft.SemanticKernel.Embeddings;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using OllamaSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Whisper.net;
using Whisper.net.Ggml;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal static class Program
    {
        private enum RunMode { Client, Server }

        //NOTE: Keep Ggml text for refrence
        //private const GgmlType DefaultGgmlType = GgmlType.Base;
        //private const string DefaultModelPath = "ggml-base.bin";
        private const GgmlType DefaultGgmlType = GgmlType.Medium;
        private const string DefaultModelPath = "ggml-medium.bin";
        private const string BaseFolder = @"C:\Temp\SOL Video Processing\";
        private const string FfmpegPath = @"C:\ffmpeg\bin\ffmpeg.exe";

        static async Task Main(string[] args)
        {
            var mode = ParseMode(args);
            await MainAsync(mode).ConfigureAwait(false);
        }

        static async Task MainAsync(RunMode mode)
        {

            mode = RunMode.Server;
            Console.WriteLine($"Starting in mode: {mode}");

            // create reusable OneDriveDownloader instance and Whisper factory
            using var downloader = new OneDriveDownloader();



            var serverDownloader = new ServerDownloader(downloader);
            await serverDownloader.DownloadAndCopy();
            return;



            //Console.WriteLine($"Starting in mode: {mode}");
            //ConfigureFfmpeg();
            //await BuildRagDatabase("C:\\Temp\\SOL RAG Test\\rag.mp4", "C:\\Temp\\SOL RAG Test\\rag.srt");

            //return;


            if (mode == RunMode.Client)
            {

                InitializeLogging(BaseFolder);
                ConfigureFfmpeg();

                if (!CheckFFmpegExists(FfmpegPath))
                {
                    Console.WriteLine($"FFMPeg not found at: {FfmpegPath}");
                    return;
                }

                var modelPath = DefaultModelPath;
                if (!File.Exists(modelPath))
                {
                    await DownloadModelAsync(modelPath, DefaultGgmlType).ConfigureAwait(false);
                }

                // Client mode: process local videos and upload results
                var mp4Files = Directory.GetFiles(BaseFolder, "*.mp4", SearchOption.TopDirectoryOnly);
                if (mp4Files.Length == 0)
                {
                    Console.WriteLine($"No .mp4 files found in folder: {BaseFolder}");
                    return;
                }

                using var factory = WhisperFactory.FromPath(modelPath);

                foreach (var videoPath in mp4Files)
                {
                    try
                    {
                        await ProcessVideoAsync(factory, videoPath, BaseFolder, downloader).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing '{videoPath}': {ex.Message}");
                    }
                }

                Console.WriteLine("Client mode work completed.");
                return;

            }
            else
            {
                // Server mode:
                // - Get listing from OneDrive folder ("AutomatedUploads")
                // - Filter filenames before download: must be "<integer>.mp4"
                // - Download each matching mp4 one-at-a-time (do NOT download ZIPs)
                // - Process downloaded mp4
                // - Move the OneDrive file into "AutomatedUploads/processed" on success
                // - Stop processing on any failure

                const string oneDriveFolder = "VideoUploads";
                const string processedSubFolder = "processed";

                var remaining = await downloader.ListFilesAsync(oneDriveFolder).ConfigureAwait(false);
                if (remaining is null || remaining.Count == 0)
                {
                    Console.WriteLine($"No files found in OneDrive folder: {oneDriveFolder}");
                    Console.WriteLine("Server mode work completed.");
                    return;
                }

                // Ensure local download dir exists
                var downloadPath = Path.Combine(BaseFolder, "Download");
                Directory.CreateDirectory(downloadPath);

                // Load model once
                var modelPath = DefaultModelPath;
                if (!File.Exists(modelPath))
                {
                    await DownloadModelAsync(modelPath, DefaultGgmlType).ConfigureAwait(false);
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
                        // NOTE: assumes OneDriveDownloader exposes DownloadFileAsync(folder, remoteName, localPath)
                        // Returns bool indicating success. If your implementation differs, adapt accordingly.
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
                            await ProcessVideoAsync(factory, localMp4Path, BaseFolder, downloader).ConfigureAwait(false);
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

                Console.WriteLine("Server mode work completed.");
                return;
            }
        }


        public static async Task BuildRagDatabase(string videoPath, string srtPath)
        {

            var collectionName = "6b0ac5fd-fc60-4cff-9fb1-bcee64832128";

            var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"), "moondream");
            var _embeddingService = ollamaClient.AsTextEmbeddingGenerationService();

            var srtService = new SrtProcessingService();
            var frameService = new VideoFrameService();
            var visionService = new OllamaVisionService();
            var qdrantVectorService = new QdrantVectorService(collectionName);

            //var chatService = new ChatService(qdrantVectorService, _embeddingService);
            //var respone = await chatService.AskAiAboutLesson("can you summarise this lesson?");
            //return;

            await qdrantVectorService.DeleteCollectionAsync();
            await qdrantVectorService.InitializeAsync();

            var segments = srtService.ParseToLessonSegments(srtPath, "Lesson1");
          
            ulong lastHash = 0;
            string lastDescription = "No visual data available.";

            foreach (var segment in segments)
            {
                //Add image hashing so we dont process the image again

                string framePath = await frameService.ExtractFrameAsync(videoPath, (int)segment.StartTime, @"C:\Temp\SOL RAG Test\frames");
             
                using var img = Image.Load<Rgb24>(framePath);
                ulong currentHash = VisualHasher.ComputeDifferenceHash(img);

                // 2. Visual Hashing Check (Threshold of 5 bits for Moondream)
                if (VisualHasher.GetSimilarityDistance(lastHash, currentHash) > 5)
                {
                    // The slide changed! Ask Moondream for a fresh description
                    // We use a 'Balanced' prompt tailored for Moondream's architecture
                    lastDescription = await visionService.DescribeFrameAsync(framePath);
                    lastHash = currentHash;

                  
                }

                // 3. Update the LessonSegment
                segment.SlideDescription = lastDescription;

                // 4. Create a Hybrid Search String
                // Merging Transcript (Audio) + SlideDescription (Visual)
                string ragContent = $"[TRANSCRIPT]: {segment.Transcript}\n[SLIDE]: {segment.SlideDescription}";
                Console.WriteLine(ragContent);

  
                segment.Vector = await _embeddingService.GenerateEmbeddingAsync(ragContent);
                Console.WriteLine(segment.Vector.ToString());
                await qdrantVectorService.UpsertVectorAsync(segment);
            }
        }

        private static void InitializeLogging(string baseFolder)
        {
            Directory.CreateDirectory(Path.Combine(baseFolder, "logs"));
            var logFile = Path.Combine(baseFolder, "logs", $"console_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            ConsoleTee.Enable(logFile);
        }

        private static void ConfigureFfmpeg()
        {
            GlobalFFOptions.Configure(options =>
            {
                options.BinaryFolder = @"C:\ffmpeg\bin";
                options.TemporaryFilesFolder = Path.GetTempPath();
            });
        }

        private static async Task ProcessVideoAsync(WhisperFactory factory, string videoPath, string baseFolder, OneDriveDownloader downloader)
        {
            var fileName = Path.GetFileName(videoPath);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            Console.WriteLine($"\nProcessing: {fileName}");

            var outputFolder = Path.Combine(baseFolder, nameWithoutExt);
            var outputFolderM3U8 = Path.Combine(baseFolder, outputFolder, "m3u8");
            var outputFolderM3U8Signed = Path.Combine(baseFolder, outputFolder, "m3u8-signed");

            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);

            Directory.CreateDirectory(outputFolder);
            Directory.CreateDirectory(outputFolderM3U8);
            Directory.CreateDirectory(outputFolderM3U8Signed);

            var mp4Path = Path.Combine(outputFolder, nameWithoutExt + ".mp4");
            var mp3Path = Path.Combine(outputFolder, nameWithoutExt + ".mp3");
            var srtPath = Path.Combine(outputFolder, nameWithoutExt + ".srt");
            var vttPath = Path.Combine(outputFolder, nameWithoutExt + ".vtt");
            var thumbImage = Path.Combine(outputFolderM3U8, nameWithoutExt + ".jpg");
            var m3u8File = Path.Combine(outputFolderM3U8, nameWithoutExt + ".m3u8");

            // Copy main file for transport
            File.Copy(videoPath, mp4Path);

            // Convert Encrypted .mu3u files
            var encodingArgsThumb = $"-i \"{videoPath}\" -ss 00:00:00 -frames:v 1 \"{thumbImage}\"";
            var encodingArgsM3U8 = $"-i \"{videoPath}\" -vcodec h264 -b:a 96k -start_number 0 -hls_time 10 -hls_list_size 0 -f hls -hls_enc 1 \"{m3u8File}\"";

            // TODO: add signed processing

            var result = await EncodeFFMpeg(encodingArgsThumb);
            if (result)
            {
                result = await EncodeFFMpeg(encodingArgsM3U8);
            }

            if (!result)
            {
                Console.WriteLine("FFMpeg Encoiding failed");
                return;
            }

            EnsureAudioExtracted(videoPath, mp3Path);

            // Use a processor per file and dispose promptly
            using var processor = factory.CreateBuilder()
                .WithLanguage("en")
                .WithProbabilities()
                .Build();

            string tempWavPath = null;
            try
            {
                // Convert to a temporary WAV file on disk to avoid large in-memory buffers
                tempWavPath = ConvertMp3ToWav(mp3Path, nameWithoutExt);

                // Transcribe and write SRT/VTT incrementally to avoid storing all segments in memory
                await TranscribeAndWriteSubtitlesAsync(processor, tempWavPath, srtPath, vttPath).ConfigureAwait(false);

                Console.WriteLine($"\nWebVTT file created: {vttPath}");

                var destDir = Path.Combine(baseFolder, nameWithoutExt);
                Directory.CreateDirectory(destDir);

                var destVideoPath = Path.Combine(destDir, Path.GetFileName(videoPath));

                try
                {
                    var zipPath = CreateZipForFolder(destDir);
                    if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
                    {
                        Console.WriteLine($"Created zip: {zipPath}");

                        // Delete the folder the zip was created from (best-effort)
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
                            var uploaded = await downloader.UploadFileToOneDriveAsync(zipPath, "AutomatedUploads").ConfigureAwait(false);
                            if (uploaded)
                            {
                                Console.WriteLine("Upload to OneDrive succeeded.");

                                // Move the zip to the local /complete folder after successful upload
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
                // Clean up temporary WAV if created
                if (!string.IsNullOrEmpty(tempWavPath) && File.Exists(tempWavPath))
                {
                    try { File.Delete(tempWavPath); }
                    catch { /* best-effort cleanup */ }
                }
            }
        }

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

        private static string ConvertMp3ToWav(string mp3Path, string nameWithoutExt)
        {
            Console.WriteLine("Converting to WAV (temporary file)");
            var tempWavPath = Path.Combine(Path.GetTempPath(), $"{nameWithoutExt}_{Guid.NewGuid():N}.wav");

            using var fileStream = File.OpenRead(mp3Path);
            using var reader = new Mp3FileReader(fileStream);
            var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);

            // Use the ISampleProvider overload directly
            WaveFileWriter.CreateWaveFile16(tempWavPath, resampler);

            return tempWavPath;
        }

        private static async Task TranscribeAndWriteSubtitlesAsync(WhisperProcessor processor, string wavPath, string srtPath, string vttPath)
        {
            using var wavStream = File.OpenRead(wavPath);

            // Open both writers
            await using var srtWriter = new StreamWriter(srtPath, false, Encoding.UTF8);
            await using var vttWriter = new StreamWriter(vttPath, false, Encoding.UTF8);

            // Write VTT header
            await vttWriter.WriteLineAsync("WEBVTT").ConfigureAwait(false);
            await vttWriter.WriteLineAsync().ConfigureAwait(false);

            var index = 1;
            await foreach (var segment in processor.ProcessAsync(wavStream))
            {
                // SRT requires numeric index and SRT time format (comma)
                await srtWriter.WriteLineAsync(index.ToString()).ConfigureAwait(false);
                await srtWriter.WriteLineAsync($"{FormatSrtTimestamp(segment.Start)} --> {FormatSrtTimestamp(segment.End)}").ConfigureAwait(false);
                await srtWriter.WriteLineAsync(segment.Text.Trim()).ConfigureAwait(false);
                await srtWriter.WriteLineAsync().ConfigureAwait(false);

                // VTT: numeric indexes aren't required — write cue directly using dot-millisecond format
                await vttWriter.WriteLineAsync($"{FormatVttTimestamp(segment.Start)} --> {FormatVttTimestamp(segment.End)}").ConfigureAwait(false);
                await vttWriter.WriteLineAsync(segment.Text.Trim()).ConfigureAwait(false);
                await vttWriter.WriteLineAsync().ConfigureAwait(false);

                index++;
            }
        }

        private static string FormatSrtTimestamp(TimeSpan time)
        {
            // SRT uses comma as millisecond separator and always hours:minutes:seconds,milliseconds
            return $"{time:hh\\:mm\\:ss},{time.Milliseconds:D3}";
        }

        private static string FormatVttTimestamp(TimeSpan time)
        {
            // VTT uses period for milliseconds
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

        private static async Task DownloadModelAsync(string fileName, GgmlType ggmlType)
        {
            Console.WriteLine($"Downloading Model {fileName}");
            await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType).ConfigureAwait(false);
            await using var fileWriter = File.OpenWrite(fileName);
            await modelStream.CopyToAsync(fileWriter).ConfigureAwait(false);
        }

        private static bool CheckFFmpegExists(string ffmpegPath)
        {
            // If it's a full path, check if file exists
            if (Path.IsPathRooted(ffmpegPath))
            {
                return File.Exists(ffmpegPath);
            }

            // If it's just "ffmpeg", check if it's in PATH
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> EncodeFFMpeg(string encodingArgs)
        {
            int exitCode = await ExecuteFFmpegAsync(FfmpegPath, encodingArgs);

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

        private static async Task<int> ExecuteFFmpegAsync(string ffmpegPath, string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using (var process = Process.Start(processInfo))
            {
                // Read output in real-time
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

                // Start async reading
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                return process.ExitCode;
            }
        }

        private static RunMode ParseMode(string[] args)
        {
            if (args == null || args.Length == 0)
                return RunMode.Client; // default

            // Accept forms: "server", "client", "--mode=server", "--mode server", "-m server"
            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i].Trim();

                if (a.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
                    a.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                    a.Equals("/?", StringComparison.OrdinalIgnoreCase))
                {
                    PrintUsage();
                    return RunMode.Client;
                }

                if (a.Equals("server", StringComparison.OrdinalIgnoreCase))
                    return RunMode.Server;

                if (a.Equals("client", StringComparison.OrdinalIgnoreCase))
                    return RunMode.Client;

                if (a.StartsWith("--mode=", StringComparison.OrdinalIgnoreCase))
                {
                    var val = a.Substring("--mode=".Length).Trim();
                    if (val.Equals("server", StringComparison.OrdinalIgnoreCase)) return RunMode.Server;
                    return RunMode.Client;
                }

                if (a.Equals("--mode", StringComparison.OrdinalIgnoreCase) || a.Equals("-m", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        var next = args[i + 1].Trim();
                        if (next.Equals("server", StringComparison.OrdinalIgnoreCase)) return RunMode.Server;
                        return RunMode.Client;
                    }
                }
            }

            return RunMode.Client;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: ISAI.Lessons.WhsiperAIDemo [mode]");
            Console.WriteLine("  mode: client|server    Runs the app in client (default) or server mode.");
            Console.WriteLine("  or:  --mode=server     or -m server");
        }
    }


    /// <summary>
    /// TextWriter that tees console output to a log file while retaining console output.
    /// Use ConsoleTee.Enable(logPath) to activate and dispose the returned IDisposable when done.
    /// </summary>
    internal sealed class ConsoleTee : TextWriter, IDisposable
    {
        private readonly TextWriter _originalOut;
        private readonly TextWriter _originalError;
        private readonly StreamWriter _fileWriter;
        private readonly object _sync = new object();
        private bool _disposed;

        private ConsoleTee(string logPath)
        {
            _originalOut = Console.Out;
            _originalError = Console.Error;

            _fileWriter = new StreamWriter(new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                AutoFlush = true,
                NewLine = Environment.NewLine
            };

            Console.SetOut(this);
            Console.SetError(this);
        }

        public static ConsoleTee Enable(string logPath) => new ConsoleTee(logPath);

        public override Encoding Encoding => _originalOut?.Encoding ?? Encoding.UTF8;

        public override void Write(char value)
        {
            lock (_sync)
            {
                _originalOut?.Write(value);
                _fileWriter?.Write(value);
            }
        }

        public override void Write(string? value)
        {
            lock (_sync)
            {
                _originalOut?.Write(value);
                _fileWriter?.Write(value);
            }
        }

        public override void WriteLine()
        {
            lock (_sync)
            {
                _originalOut?.WriteLine();
                _fileWriter?.WriteLine();
            }
        }

        public override void WriteLine(string? value)
        {
            lock (_sync)
            {
                _originalOut?.WriteLine(value);
                _fileWriter?.WriteLine(value);
            }
        }

        public override void Flush()
        {
            lock (_sync)
            {
                _originalOut?.Flush();
                _fileWriter?.Flush();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                if (_originalOut != null) Console.SetOut(_originalOut);
                if (_originalError != null) Console.SetError(_originalError);
            }
            catch
            {
                // ignore
            }

            try
            {
                _fileWriter?.Flush();
                _fileWriter?.Dispose();
            }
            catch
            {
                // ignore
            }
        }
    }
}