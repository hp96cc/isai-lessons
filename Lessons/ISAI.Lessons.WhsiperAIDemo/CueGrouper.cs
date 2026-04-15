using System.Collections.Generic;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal static class CueGrouper
    {
        private static readonly System.Text.RegularExpressions.Regex SentenceEnd =
            new System.Text.RegularExpressions.Regex(@"[.!?…]$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        public static List<CueGroup> GroupCues(List<VttCue> cues, int maxCharsPerGroup = 500)
        {
            var groups = new List<CueGroup>();
            var current = new List<VttCue>();
            var charCount = 0;

            foreach (var cue in cues)
            {
                current.Add(cue);
                charCount += cue.Text.Length;

                if (charCount >= maxCharsPerGroup && SentenceEnd.IsMatch(cue.Text.TrimEnd()))
                {
                    groups.Add(BuildGroup(current));
                    current = new List<VttCue>();
                    charCount = 0;
                }
            }

            if (current.Count > 0)
            {
                groups.Add(BuildGroup(current));
            }

            return groups;
        }

        private static CueGroup BuildGroup(List<VttCue> cues)
        {
            var combined = string.Join(" ", cues.ConvertAll(c => c.Text));
            return new CueGroup(new List<VttCue>(cues), combined);
        }
    }
}
