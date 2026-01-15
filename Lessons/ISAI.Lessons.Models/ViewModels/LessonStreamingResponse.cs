using System;

namespace ISAI.Lessons.Models.ViewModels
{
    public class LessonStreamingResponse
    {
        public LessonStreamingResponse() { }

        public string StreamingUrl { get; set; }

        public string StreamingSignedUrl { get; set; }

        public string SubtitlesUrl { get; set; }

        public string SubtitlesSignedUrl { get; set; }

        public bool IsSignedAvailable { get; set; }
        public bool IsSubtitlesAvailable { get; set; }

        public bool IsSigndSubtitlesAvailable { get; set; }

        public string Token { get; set; }
    }

    public class LessonStreamingToken
    {
        public LessonStreamingToken() { }

        public int LessonId { get; set; }

        public DateTime ExpiryDate { get; set; }
    }

}
