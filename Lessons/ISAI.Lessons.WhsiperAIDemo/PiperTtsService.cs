using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal class PiperTtsService
    {
        private readonly string _piperPath;
        private readonly string _voicesPath;

        public PiperTtsService(string piperPath, string voicesPath)
        {
            _piperPath = piperPath;
            _voicesPath = voicesPath;
        }

        /// <summary>
        /// Generates a WAV file from text using the specified Piper voice model.
        /// </summary>
        /// <param name="text">The text to synthesize</param>
        /// <param name="voiceModel">Voice model name, e.g., "es_ES-davefx-medium"</param>
        /// <param name="outputWavPath">Path for the output WAV file</param>
        /// <param name="lengthScale">Speaking speed: 1.0=normal, 0.8=faster, 1.2=slower</param>
        public async Task GenerateSpeechAsync(string text, string voiceModel, string outputWavPath, double lengthScale = 1.0)
        {
            var modelPath = Path.Combine(_voicesPath, voiceModel + ".onnx");

            if (!File.Exists(modelPath))
                throw new FileNotFoundException($"Piper voice model not found: {modelPath}");

            var args = $"--model \"{modelPath}\" --output_file \"{outputWavPath}\" --length-scale {lengthScale.ToString("F2", CultureInfo.InvariantCulture)}";

            var processInfo = new ProcessStartInfo
            {
                FileName = _piperPath,
                Arguments = args,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
                throw new Exception($"Failed to start Piper process. FileName: '{_piperPath}', Arguments: '{args}'");

            await process.StandardInput.WriteLineAsync(text).ConfigureAwait(false);
            process.StandardInput.Close();

            var stderr = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
            await process.WaitForExitAsync().ConfigureAwait(false);

            if (process.ExitCode != 0)
                throw new Exception($"Piper TTS failed (exit code {process.ExitCode}): {stderr}");
        }

        /// <summary>
        /// Gets the duration of an audio file in seconds using ffprobe.
        /// </summary>
        public async Task<double> GetAudioDurationAsync(string audioPath)
        {
            var ffprobePath = GetFfprobePath();
            var args = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{audioPath}\"";

            var processInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            await process.WaitForExitAsync().ConfigureAwait(false);

            if (double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var duration))
                return duration;

            throw new Exception($"Failed to parse audio duration from ffprobe output: '{output.Trim()}'");
        }

        /// <summary>
        /// Time-fits a WAV file to a target duration using FFmpeg atempo filter.
        /// </summary>
        public async Task TimeFitAudioAsync(string inputWavPath, string outputWavPath, double targetDurationSeconds)
        {
            var actualDuration = await GetAudioDurationAsync(inputWavPath).ConfigureAwait(false);
            if (actualDuration <= 0) return;

            var ratio = targetDurationSeconds / actualDuration;

            // Clamp to safe atempo range (0.5 to 2.0 per filter instance)
            ratio = Math.Max(0.5, Math.Min(2.0, ratio));

            var ffmpegPath = Program.FfmpegPath;
            var args = $"-y -i \"{inputWavPath}\" -af \"atempo={ratio.ToString("F4", CultureInfo.InvariantCulture)}\" -t {targetDurationSeconds.ToString("F3", CultureInfo.InvariantCulture)} \"{outputWavPath}\"";

            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            var stderrTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync().ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                var stderr = await stderrTask.ConfigureAwait(false);
                Console.WriteLine($"FFmpeg atempo warning (non-fatal): {stderr}");
            }
        }

        /// <summary>
        /// Generates a silence WAV file of the specified duration.
        /// </summary>
        public async Task GenerateSilenceAsync(string outputWavPath, double durationSeconds, int sampleRate = 22050)
        {
            var ffmpegPath = Program.FfmpegPath;
            var args = $"-y -f lavfi -i anullsrc=r={sampleRate}:cl=mono -t {durationSeconds.ToString("F3", CultureInfo.InvariantCulture)} \"{outputWavPath}\"";

            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            await process.WaitForExitAsync().ConfigureAwait(false);
        }

        private static string GetFfprobePath()
        {
            var ffmpegPath = Program.FfmpegPath;
            var dir = Path.GetDirectoryName(ffmpegPath);
            if (!string.IsNullOrEmpty(dir))
            {
                var ffprobeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe";
                var ffprobePath = Path.Combine(dir, ffprobeName);
                if (File.Exists(ffprobePath))
                    return ffprobePath;
            }
            return "ffprobe";
        }
    }
}
