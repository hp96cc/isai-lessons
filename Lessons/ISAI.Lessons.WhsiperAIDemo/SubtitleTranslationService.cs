using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal class SubtitleTranslationService
    {
        private readonly string _ollamaBaseUrl;
        private readonly string _ollamaModel;
        private readonly int _maxCharsPerGroup;

        public SubtitleTranslationService(string ollamaBaseUrl, string ollamaModel, int maxCharsPerGroup = 500)
        {
            _ollamaBaseUrl = ollamaBaseUrl;
            _ollamaModel = ollamaModel;
            _maxCharsPerGroup = maxCharsPerGroup;
        }

        public async Task TranslateVttAsync(
            string vttPath,
            string fullTranscript,
            string targetLanguage,
            string outputPath,
            string lessonTitle = "",
            List<string> glossary = null)
        {
            Console.WriteLine($"  Parsing VTT: {Path.GetFileName(vttPath)}");
            var cues = VttParser.Parse(vttPath);
            Console.WriteLine($"  Parsed {cues.Count} cues.");

            var context = new TranslationContext(
                LessonTitle: lessonTitle,
                FullTranscript: fullTranscript,
                SourceLanguage: "en",
                TargetLanguage: targetLanguage,
                GlossaryTerms: glossary ?? new List<string>()
            );

            Console.WriteLine($"  Grouping cues (maxCharsPerGroup={_maxCharsPerGroup})...");
            var groups = CueGrouper.GroupCues(cues, _maxCharsPerGroup);
            Console.WriteLine($"  Created {groups.Count} cue groups.");

            var translator = new OllamaTranslator(_ollamaModel, _ollamaBaseUrl);
            var allTranslatedCues = new System.Collections.Generic.List<TranslatedCue>();

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                Console.WriteLine($"  Translating group {i + 1}/{groups.Count} ({group.CombinedText.Length} chars) -> [{targetLanguage}]...");

                var translatedText = await translator.TranslateAsync(group.CombinedText, context).ConfigureAwait(false);
                var reflowed = TimingReflowEngine.Reflow(group, translatedText);
                allTranslatedCues.AddRange(reflowed);
            }

            Console.WriteLine($"  Writing translated VTT: {outputPath}");
            VttParser.Write(outputPath, allTranslatedCues, targetLanguage);
            Console.WriteLine($"  Done: {Path.GetFileName(outputPath)}");
        }

        public async Task TranslateToAllLanguagesAsync(
            string vttPath,
            string fullTranscript,
            string lessonTitle,
            List<string> targetLanguages,
            List<string> glossary = null)
        {
            var baseName = Path.Combine(
                Path.GetDirectoryName(vttPath),
                Path.GetFileNameWithoutExtension(vttPath));

            foreach (var langCode in targetLanguages)
            {
                var outputPath = $"{baseName}.{langCode}.vtt";
                Console.WriteLine($"Translating to [{langCode}]: {outputPath}");

                try
                {
                    await TranslateVttAsync(vttPath, fullTranscript, langCode, outputPath, lessonTitle, glossary)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error translating to [{langCode}]: {ex.Message}");
                }
            }
        }
    }
}
