using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal static class VttParser
    {
        private static readonly Regex TimestampRegex = new Regex(
            @"(\d{2}:\d{2}:\d{2}\.\d{3})\s*-->\s*(\d{2}:\d{2}:\d{2}\.\d{3})",
            RegexOptions.Compiled);

        public static List<VttCue> Parse(string filePath)
        {
            var cues = new List<VttCue>();
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            var index = 1;
            var i = 0;

            while (i < lines.Length)
            {
                var line = lines[i];
                var match = TimestampRegex.Match(line);
                if (match.Success)
                {
                    var start = TimeSpan.ParseExact(match.Groups[1].Value, @"hh\:mm\:ss\.fff", null);
                    var end = TimeSpan.ParseExact(match.Groups[2].Value, @"hh\:mm\:ss\.fff", null);

                    i++;
                    var textLines = new List<string>();
                    while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        textLines.Add(lines[i]);
                        i++;
                    }

                    var text = string.Join(" ", textLines).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        cues.Add(new VttCue(index++, start, end, text));
                    }
                }
                else
                {
                    i++;
                }
            }

            return cues;
        }

        public static void Write(string outputPath, List<TranslatedCue> cues, string targetLanguage)
        {
            using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));
            writer.WriteLine("WEBVTT");
            writer.WriteLine($"Language: {targetLanguage}");
            writer.WriteLine();

            foreach (var cue in cues)
            {
                var start = FormatTimestamp(cue.OriginalCue.Start);
                var end = FormatTimestamp(cue.OriginalCue.End);
                writer.WriteLine($"{start} --> {end}");
                writer.WriteLine(cue.TranslatedText);
                writer.WriteLine();
            }
        }

        private static string FormatTimestamp(TimeSpan time)
        {
            return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
        }
    }
}
