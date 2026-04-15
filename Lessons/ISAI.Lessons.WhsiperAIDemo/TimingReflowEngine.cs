using System;
using System.Collections.Generic;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal static class TimingReflowEngine
    {
        private const int MaxLineChars = 42;

        public static List<TranslatedCue> Reflow(CueGroup group, string translatedText)
        {
            var words = translatedText.Split(new[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            var totalTranslatedWords = words.Length;
            var totalOriginalChars = 0;
            foreach (var cue in group.Cues)
                totalOriginalChars += cue.Text.Length;

            if (totalOriginalChars == 0)
                totalOriginalChars = 1;

            var result = new List<TranslatedCue>(group.Cues.Count);
            var wordIndex = 0;

            for (var i = 0; i < group.Cues.Count; i++)
            {
                var cue = group.Cues[i];
                int wordCount;

                if (i == group.Cues.Count - 1)
                {
                    // Last cue gets all remaining words
                    wordCount = totalTranslatedWords - wordIndex;
                }
                else
                {
                    wordCount = (int)Math.Round((double)cue.Text.Length / totalOriginalChars * totalTranslatedWords);
                    wordCount = Math.Max(1, wordCount);
                    wordCount = Math.Min(wordCount, totalTranslatedWords - wordIndex - (group.Cues.Count - i - 1));
                }

                wordCount = Math.Max(1, wordCount);

                var cuePhraseWords = new string[wordCount];
                Array.Copy(words, wordIndex, cuePhraseWords, 0, Math.Min(wordCount, words.Length - wordIndex));
                wordIndex += wordCount;

                var cueText = string.Join(" ", cuePhraseWords);
                cueText = InsertLineBreakIfNeeded(cueText);

                result.Add(new TranslatedCue(cue, cueText));
            }

            return result;
        }

        private static string InsertLineBreakIfNeeded(string text)
        {
            if (text.Length <= MaxLineChars)
                return text;

            var midpoint = text.Length / 2;
            // Search for nearest word boundary around midpoint
            var bestBreak = -1;
            for (var radius = 0; radius <= midpoint; radius++)
            {
                var left = midpoint - radius;
                var right = midpoint + radius;

                if (right < text.Length && text[right] == ' ')
                {
                    bestBreak = right;
                    break;
                }
                if (left > 0 && text[left] == ' ')
                {
                    bestBreak = left;
                    break;
                }
            }

            if (bestBreak < 0)
                return text;

            return text.Substring(0, bestBreak) + "\n" + text.Substring(bestBreak + 1);
        }
    }
}
