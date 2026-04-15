using System;
using System.Collections.Generic;

namespace ISAI.Lessons.WhsiperAIDemo
{
    public record VttCue(int Index, TimeSpan Start, TimeSpan End, string Text);

    public record CueGroup(List<VttCue> Cues, string CombinedText);

    public record TranslatedCue(VttCue OriginalCue, string TranslatedText);

    public record TranslationContext(
        string LessonTitle,
        string FullTranscript,
        string SourceLanguage,
        string TargetLanguage,
        List<string> GlossaryTerms
    );
}
