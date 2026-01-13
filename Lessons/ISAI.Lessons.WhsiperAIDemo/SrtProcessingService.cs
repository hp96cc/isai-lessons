using Microsoft.Extensions.VectorData;
using SubtitlesParserV2;
using System.Text;


namespace ISAI.Lessons.WhsiperAIDemo
{
 
    public record SrtItem(TimeSpan Start, TimeSpan End, string Text);




public class LessonSegment
    {
        
        [VectorStoreKey]
        public ulong Id { get; set; }

        [VectorStoreData]
        public double StartTime { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string VideoId { get; set; }

        [VectorStoreData]
        public string Transcript { get; set; }

        [VectorStoreData]
        public string SlideDescription { get; set; }

        [VectorStoreVector(2048)]
        public ReadOnlyMemory<float> Vector { get; set; }

        /// <summary>
        /// Helper to get a unified string for embedding generation
        /// </summary>
        public string GetSearchableText() =>
            $"Transcript: {Transcript} | Visual Context: {SlideDescription}";
    }

    public class SrtProcessingService
    {
        public List<LessonSegment> ParseToLessonSegments(string srtPath, string videoId)
        {

            using var fileStream = File.OpenRead(srtPath);
            var subtitleResultModel = SubtitleParser.ParseStream(fileStream, Encoding.UTF8);

            var segments = new List<LessonSegment>();
            var currentTranscript = new StringBuilder();
            double segmentStartTime = 0;
            const double targetSegmentDuration = 20.0; // 20-second chunks for RAG

            foreach (var item in subtitleResultModel.Subtitles)
            {
                if (currentTranscript.Length == 0) segmentStartTime = item.StartTime / 1000.0;

                // Combine lines into a single transcript block
                string cleanText = string.Join(" ", item.Lines).Trim();
                currentTranscript.Append(cleanText + " ");

                double currentTime = item.EndTime / 1000.0;

                // If we've reached our time limit, finalize this segment
                if (currentTime - segmentStartTime >= targetSegmentDuration)
                {
                    segments.Add(new LessonSegment
                    {
                        Id = (ulong) DateTime.Now.Ticks,
                        StartTime = segmentStartTime,
                        Transcript = currentTranscript.ToString().Trim(),
                        VideoId = videoId,
                        SlideDescription = "Pending Analysis..." // Filled later by Moondream
                    });
                    currentTranscript.Clear();
                }
            }

            // Add any remaining text as a final segment
            if (currentTranscript.Length > 0)
            {
                segments.Add(new LessonSegment
                {
                    StartTime = segmentStartTime,
                    Transcript = currentTranscript.ToString().Trim(),
                    VideoId = videoId
                });
            }

            return segments;
        }
    }
}