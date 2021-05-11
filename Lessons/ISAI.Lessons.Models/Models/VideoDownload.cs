using ISAI.Lessons.Models.Enums;
using System;

namespace ISAI.Lessons.Models.Models
{
    public class VideoDownload
    {

        [SQLite.PrimaryKey]
        public Guid Id { get; set; }

        public string LessonName { get; set; }

        public string LessonGroup { get; set; }

        public int LessonId { get; set; }

        public string DownloadUrl { get; set; }

        public string DownloadId { get; set; }

        public DateTime DateDownloaded { get; set; }

        public VideoDownloadStatusCode VideoDownloadStatusCode { get; set; }

        public long TotalBytes {get;set; }

        public long TotalBytesDownloaded { get; set; }
    }
}
