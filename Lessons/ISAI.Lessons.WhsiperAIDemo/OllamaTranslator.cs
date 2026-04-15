using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal class OllamaTranslator
    {
        private readonly string _model;
        private readonly HttpClient _httpClient;

        private static readonly Dictionary<string, string> LanguageNames = new Dictionary<string, string>
        {
            { "es", "Spanish" },
            { "fr", "French" },
            { "de", "German" },
            { "pt", "Portuguese" },
            { "it", "Italian" },
            { "nl", "Dutch" },
            { "pl", "Polish" },
            { "ar", "Arabic" },
            { "zh", "Chinese (Simplified)" },
            { "ja", "Japanese" },
            { "ko", "Korean" },
            { "hi", "Hindi" },
            { "tr", "Turkish" },
            { "ru", "Russian" },
            { "sv", "Swedish" }
        };

        private static readonly Regex PrefixPattern = new Regex(
            @"^(Here is the translation[:\s]*|Translation[:\s]*|Translated text[:\s]*|Here's the translation[:\s]*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public OllamaTranslator(string model, string baseUrl)
        {
            _model = model;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromMinutes(10)
            };
        }

        public async Task<string> TranslateAsync(string textBlock, TranslationContext context)
        {
            var targetLanguageName = LanguageNames.TryGetValue(context.TargetLanguage, out var name)
                ? name
                : context.TargetLanguage;

            var truncatedTranscript = context.FullTranscript.Length > 3000
                ? context.FullTranscript.Substring(0, 3000)
                : context.FullTranscript;

            var systemPrompt =
                $"You are a professional subtitle translator for educational video lessons. " +
                $"Translate the provided subtitle segment into {targetLanguageName}. " +
                $"Output ONLY the translated text with no explanations, prefixes, or additional commentary. " +
                $"Match the approximate length of the source text as these are subtitles in fixed time windows. " +
                $"Keep technical terms, acronyms, and proper nouns as-is.";

            var userPrompt =
                $"CONTEXT (DO NOT translate this — it is provided for reference only):\n{truncatedTranscript}\n\n" +
                $"Translate the following subtitle segment into {targetLanguageName}:\n{textBlock}";

            var requestBody = new
            {
                model = _model,
                stream = false,
                options = new { temperature = 0.2 },
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/api/chat", requestBody).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>().ConfigureAwait(false);
            var translated = result?.Message?.Content ?? textBlock;

            return StripPrefixes(translated.Trim());
        }

        private static string StripPrefixes(string text)
        {
            return PrefixPattern.Replace(text, string.Empty).Trim();
        }

        private record OllamaChatMessage(string Role, string Content);
        private record OllamaChatResponse(OllamaChatMessage Message, bool Done);
    }
}
