using System;
using System.Collections.Generic;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class LessonHistoryResponseViewModel
    {
        public List<LessonHistory> LessonHistory { get; set; }
    }

    public class LessonHistory
    {
        public int LessonId { get; set; }

        public string LessonName { get; set; }

        public string LessonGroupName { get; set; }

        public DateTimeOffset DateTimeStart { get; set; }

        public DateTimeOffset DateTimeEnd { get; set; }

        public int WatchedDurationInMinutes { get; set; }

    }
}
