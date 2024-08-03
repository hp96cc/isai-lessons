using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.ViewModels
{
    public class LessonStreamingResponse
    {
        public string StreamingUrl { get; set; }

        public string Token { get; set; }
    }

    public class LessonStreamingToken
    {
        public int LessonId { get; set; }

        public DateTime ExpiryDate { get; set; }
    }

}
