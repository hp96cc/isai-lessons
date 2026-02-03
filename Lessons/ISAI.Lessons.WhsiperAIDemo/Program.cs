using FFMpegCore;
using System.Diagnostics;
using Whisper.net.Ggml;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal static class Program
    {
        private enum RunMode { DownloadAndProcessVideo, DownloadAndMoveToServerLocation }

        internal static readonly GgmlType DefaultGgmlType = GgmlType.Medium;
        
        internal static string DefaultModelPath => AppConfiguration.Instance.Application.DefaultModelPath;
        internal static string BaseFolder => AppConfiguration.Instance.Application.BaseFolder;
        internal static string FfmpegPath => AppConfiguration.Instance.Application.FfmpegPath;

        // Application entry point. Parses run mode then delegates to async main.
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Platform: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
            Console.WriteLine($"Base Folder: {BaseFolder}");
            Console.WriteLine($"FFmpeg Path: {FfmpegPath}");
            Console.WriteLine();

            var mode = ParseMode(args);
            await MainAsync(mode).ConfigureAwait(false);
        }

        // Primary async startup. Chooses processing mode and creates
        // a shared OneDriveDownloader instance used for operations.
        static async Task MainAsync(RunMode mode)
        {
            // Force DownloadAndProcessVideo for local dev/testing; change as needed.
            mode = RunMode.DownloadAndProcessVideo;
            Console.WriteLine($"Starting in mode: {mode}");

            //DO NOT DELETE - TESTING PURPOSES ONLY
            //var serverDownloader = new ServerDownloader(downloader);
            //await serverDownloader.DownloadAndCopy();
            //return;

            //Console.WriteLine($"Starting in mode: {mode}");
            //ConfigureFfmpeg();
            //await BuildRagDatabase("C:\\Temp\\SOL RAG Test\\rag.mp4", "C:\\Temp\\SOL RAG Test\\rag.srt");

            //return;


            // Create a single downloader instance that's reused for operations.
            using var downloader = new OneDriveDownloader();

            if (mode == RunMode.DownloadAndProcessVideo)
            {
                await ServerRunner.RunAsync(downloader).ConfigureAwait(false);
                Console.WriteLine("DownloadAndProcessVideo work completed.");
                return;
            }

            if (mode == RunMode.DownloadAndMoveToServerLocation)
            {
                var serverDownloader = new ServerDownloader(downloader);
                await serverDownloader.DownloadAndCopy().ConfigureAwait(false);
                Console.WriteLine("DownloadAndMoveToServerLocation work completed.");
                return;
            }
        }

        // Delegates to the new BuildRagRunner to keep the original Program API stable.
        public static Task BuildRagDatabase(string videoPath, string srtPath)
            => BuildRagRunner.BuildRagDatabase(videoPath, srtPath);

        internal static void InitializeLogging(string baseFolder)
        {
            Directory.CreateDirectory(Path.Combine(baseFolder, "logs"));
            var logFile = Path.Combine(baseFolder, "logs", $"console_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            ConsoleTee.Enable(logFile);
        }

        internal static void ConfigureFfmpeg()
        {
            var ffmpegDir = Path.GetDirectoryName(FfmpegPath);
            GlobalFFOptions.Configure(options =>
            {
                if (!string.IsNullOrEmpty(ffmpegDir))
                {
                    options.BinaryFolder = ffmpegDir;
                }
                options.TemporaryFilesFolder = Path.GetTempPath();
            });
        }

        // --- Remaining helper methods that are not part of ProcessVideoService remain below ---
        // (DownloadModelAsync, CheckFFmpegExists, etc. are still implemented here as before)

        internal static async Task DownloadModelAsync(string fileName, GgmlType ggmlType)
        {
            Console.WriteLine($"Downloading Model {fileName}");
            await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType).ConfigureAwait(false);
            await using var fileWriter = File.OpenWrite(fileName);
            await modelStream.CopyToAsync(fileWriter).ConfigureAwait(false);
        }

        internal static bool CheckFFmpegExists(string ffmpegPath)
        {
            // If it's a full path, check if file exists
            if (Path.IsPathRooted(ffmpegPath))
            {
                return File.Exists(ffmpegPath);
            }

            // If it's just "ffmpeg", check if it's in PATH by running ffmpeg -version
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

        internal static async Task<bool> EncodeFFMpeg(string encodingArgs)
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

        internal static async Task<int> ExecuteFFmpegAsync(string ffmpegPath, string arguments)
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
                // Read output in real-time and forward to console (and log if ConsoleTee enabled)
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
            // Default to DownloadAndProcessVideo
            if (args == null || args.Length == 0)
                return RunMode.DownloadAndProcessVideo;

            // Accept forms: "server" (back-compat), "downloadandprocessvideo", "downloadandmovetoserverlocation",
            // "--mode=downloadandprocessvideo", "--mode downloadandmovetoserverlocation", "-m downloadandmovetoserverlocation"
            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i].Trim();

                if (a.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
                    a.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                    a.Equals("/?", StringComparison.OrdinalIgnoreCase))
                {
                    PrintUsage();
                    return RunMode.DownloadAndProcessVideo;
                }

                // Backwards compatibility: accept "server" as DownloadAndProcessVideo
                if (a.Equals("server", StringComparison.OrdinalIgnoreCase))
                    return RunMode.DownloadAndProcessVideo;

                if (a.Equals("downloadandprocessvideo", StringComparison.OrdinalIgnoreCase) ||
                    a.Equals("downloadandprocess", StringComparison.OrdinalIgnoreCase) ||
                    a.Equals("process", StringComparison.OrdinalIgnoreCase))
                {
                    return RunMode.DownloadAndProcessVideo;
                }

                if (a.Equals("downloadandmovetoserverlocation", StringComparison.OrdinalIgnoreCase) ||
                    a.Equals("downloadandmovetoserver", StringComparison.OrdinalIgnoreCase) ||
                    a.Equals("movetoserver", StringComparison.OrdinalIgnoreCase) ||
                    a.Equals("move", StringComparison.OrdinalIgnoreCase))
                {
                    return RunMode.DownloadAndMoveToServerLocation;
                }

                if (a.StartsWith("--mode=", StringComparison.OrdinalIgnoreCase))
                {
                    var val = a.Substring("--mode=".Length).Trim();
                    // normalize same as above
                    if (val.Equals("downloadandmovetoserverlocation", StringComparison.OrdinalIgnoreCase) ||
                        val.Equals("movetoserver", StringComparison.OrdinalIgnoreCase) ||
                        val.Equals("move", StringComparison.OrdinalIgnoreCase))
                        return RunMode.DownloadAndMoveToServerLocation;

                    return RunMode.DownloadAndProcessVideo;
                }

                if (a.Equals("--mode", StringComparison.OrdinalIgnoreCase) || a.Equals("-m", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        var next = args[i + 1].Trim();
                        if (next.Equals("downloadandmovetoserverlocation", StringComparison.OrdinalIgnoreCase) ||
                            next.Equals("movetoserver", StringComparison.OrdinalIgnoreCase) ||
                            next.Equals("move", StringComparison.OrdinalIgnoreCase))
                            return RunMode.DownloadAndMoveToServerLocation;

                        return RunMode.DownloadAndProcessVideo;
                    }
                }
            }

            return RunMode.DownloadAndProcessVideo;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: ISAI.Lessons.WhsiperAIDemo [mode]");
            Console.WriteLine("  mode: DownloadAndProcessVideo|DownloadAndMoveToServerLocation");
            Console.WriteLine("    DownloadAndProcessVideo    - download MP4s, transcribe/process and upload results (formerly 'server').");
            Console.WriteLine("    DownloadAndMoveToServerLocation - download ZIP uploads and move contents to server location.");
            Console.WriteLine("  or:  --mode=DownloadAndProcessVideo     or -m DownloadAndProcessVideo");
        }
    }



}