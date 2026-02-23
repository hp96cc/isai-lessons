namespace ISAI.Lessons.Models.Models.App
{
    public class Lesson : BaseModel
    {

        public int AppId { get; set; }

        public int LessonGroupId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string LessonNotes { get; set; }

        public int ListOrder { get; set; }

        public string AssetId { get; set; }

        public string SourceUrl { get; set; }

        public bool PendingDownload { get; set; }

        public string BitmovinId { get; set; }
        public bool IsSignedAvailable { get; set; }
        public bool IsSubtitlesAvailable { get; set; }

        public bool IsSigndSubtitlesAvailable { get; set; }



    }
}
