using System;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class TutorialCreateRequestViewModel
    {
        public string TutorId { get; set; }
        public int CustomerId { get; set; }
        public int? LessonId { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public int Duration { get; set; }
        public string CustomerNotes { get; set; }

    }
}
