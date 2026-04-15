using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal class AudioDubbingService
    {
        private readonly PiperTtsService _tts;

        public AudioDubbingService(PiperTtsService tts)
        {
            _tts = tts;
        }

        /// <summary>
        /// Generates a dubbed MP4 from a translated VTT and the original MP4 video.
        /// </summary>
        public async Task GenerateDubbedVideoAsync(
            string translatedVttPath,
            string originalMp4Path,
            string outputMp4Path,
            string voiceModel,
            string languageCode)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"piper_dub_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // 1. Parse translated VTT
                var cues = VttParser.Parse(translatedVttPath);
                Console.WriteLine($"    Parsed {cues.Count} cues for dubbing");

                // 2. Generate speech for each cue with time-fitting
                var segments = new List<string>();

                // Handle silence before first cue (if it doesn't start at 0:00)
                if (cues.Count > 0 && cues[0].Start.TotalSeconds > 0.1)
                {
                    var leadSilencePath = Path.Combine(tempDir, "silence_lead.wav");
                    await _tts.GenerateSilenceAsync(leadSilencePath, cues[0].Start.TotalSeconds).ConfigureAwait(false);
                    segments.Add(leadSilencePath);
                }

                for (var i = 0; i < cues.Count; i++)
                {
                    var cue = cues[i];
                    var cueDuration = (cue.End - cue.Start).TotalSeconds;
                    if (cueDuration <= 0) continue;

                    var rawWavPath = Path.Combine(tempDir, $"cue_{i:D4}_raw.wav");
                    var fittedWavPath = Path.Combine(tempDir, $"cue_{i:D4}_fitted.wav");

                    Console.WriteLine($"    Generating speech for cue {i + 1}/{cues.Count} ({cueDuration:F1}s)...");

                    // Generate at normal speed first
                    await _tts.GenerateSpeechAsync(cue.Text, voiceModel, rawWavPath).ConfigureAwait(false);

                    // Measure actual duration
                    var actualDuration = await _tts.GetAudioDurationAsync(rawWavPath).ConfigureAwait(false);
                    var ratio = cueDuration / actualDuration;

                    // Hybrid approach: if within ±15%, use FFmpeg atempo. Otherwise, re-generate with adjusted length_scale.
                    if (ratio >= 0.85 && ratio <= 1.15)
                    {
                        await _tts.TimeFitAudioAsync(rawWavPath, fittedWavPath, cueDuration).ConfigureAwait(false);
                    }
                    else
                    {
                        // Re-generate with Piper's --length-scale for more natural speech
                        // length_scale > 1.0 = slower, < 1.0 = faster
                        var lengthScale = 1.0 / ratio;
                        lengthScale = Math.Max(0.5, Math.Min(2.0, lengthScale));

                        var rescaledWavPath = Path.Combine(tempDir, $"cue_{i:D4}_rescaled.wav");
                        await _tts.GenerateSpeechAsync(cue.Text, voiceModel, rescaledWavPath, lengthScale).ConfigureAwait(false);

                        // Still time-fit the result to exact duration
                        await _tts.TimeFitAudioAsync(rescaledWavPath, fittedWavPath, cueDuration).ConfigureAwait(false);

                        try { File.Delete(rescaledWavPath); } catch { }
                    }

                    segments.Add(fittedWavPath);

                    try { File.Delete(rawWavPath); } catch { }

                    // Generate silence gap between this cue and the next
                    if (i + 1 < cues.Count)
                    {
                        var gapDuration = (cues[i + 1].Start - cue.End).TotalSeconds;
                        if (gapDuration > 0.05)
                        {
                            var silencePath = Path.Combine(tempDir, $"silence_{i:D4}.wav");
                            await _tts.GenerateSilenceAsync(silencePath, gapDuration).ConfigureAwait(false);
                            segments.Add(silencePath);
                        }
                    }
                }

                // 3. Concatenate all segments using FFmpeg concat demuxer
                var concatListPath = Path.Combine(tempDir, "concat_list.txt");
                var fullAudioPath = Path.Combine(tempDir, "full_audio.wav");

                using (var writer = new StreamWriter(concatListPath))
                {
                    foreach (var segment in segments)
                    {
                        if (File.Exists(segment))
                        {
                            writer.WriteLine($"file '{segment.Replace("'", "'\\''")}'");
                        }
                    }
                }

                var concatArgs = $"-y -f concat -safe 0 -i \"{concatListPath}\" -c copy \"{fullAudioPath}\"";
                await ExecuteProcessAsync(Program.FfmpegPath, concatArgs).ConfigureAwait(false);

                // 4. Mux onto original MP4 (video copy, new audio)
                Console.WriteLine($"    Muxing dubbed audio onto video: {Path.GetFileName(outputMp4Path)}");
                var muxArgs = $"-y -i \"{originalMp4Path}\" -i \"{fullAudioPath}\" -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 -shortest \"{outputMp4Path}\"";
                await ExecuteProcessAsync(Program.FfmpegPath, muxArgs).ConfigureAwait(false);

                Console.WriteLine($"    ✓ Dubbed MP4 created: {Path.GetFileName(outputMp4Path)}");
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, recursive: true);
                }
                catch { /* best-effort cleanup */ }
            }
        }

        /// <summary>
        /// Generates dubbed MP4s for all configured target languages.
        /// </summary>
        public async Task GenerateAllDubbedVideosAsync(
            string originalMp4Path,
            string outputFolder,
            string nameWithoutExt,
            List<string> targetLanguages,
            Dictionary<string, string> voiceMap)
        {
            foreach (var lang in targetLanguages)
            {
                var translatedVttPath = Path.Combine(outputFolder, $"{nameWithoutExt}.{lang}.vtt");
                if (!File.Exists(translatedVttPath))
                {
                    Console.WriteLine($"  [{lang.ToUpper()}] Skipping dubbing — translated VTT not found: {translatedVttPath}");
                    continue;
                }

                if (!voiceMap.TryGetValue(lang, out var voiceModel))
                {
                    Console.WriteLine($"  [{lang.ToUpper()}] Skipping dubbing — no voice model configured in VoiceMap");
                    continue;
                }

                var outputMp4Path = Path.Combine(outputFolder, $"{nameWithoutExt}.{lang}.mp4");
                Console.WriteLine($"  [{lang.ToUpper()}] Generating dubbed MP4...");

                try
                {
                    await GenerateDubbedVideoAsync(translatedVttPath, originalMp4Path, outputMp4Path, voiceModel, lang)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [{lang.ToUpper()}] Dubbing failed: {ex.Message}");
                }
            }
        }

        private static async Task<int> ExecuteProcessAsync(string fileName, string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            // Read stdout and stderr concurrently to prevent buffer deadlocks
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync().ConfigureAwait(false);
            await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);
            return process.ExitCode;
        }
    }
}
