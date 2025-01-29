using System;
using ISAI.Lessons.Models.Enums;

namespace ISAI.Lessons.Mobile.Models.Messages
{
    public class DownloadCompleteMessage
    {
        public DownloadCompleteMessage()
        {
           
        }

        public int LessonId { get; set; }

        public VideoDownloadStatusCode VideoDownloadStatusCode { get; set; }
    }
}
