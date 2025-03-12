using System;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class TutorialPurchaseRequestViewModel
    {
        public string TutorId { get; set; }
        public int TutorialSubjectId { get; set; }
        public int? LessonId { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public string CustomerNotes { get; set; }
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }

    }
}
