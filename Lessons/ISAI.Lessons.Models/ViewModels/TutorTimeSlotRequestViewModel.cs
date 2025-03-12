using System;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class TutorTimeSlotRequestViewModel 
    {
        public string TutorId { get; set; }
        public DateTimeOffset TutorialDate { get; set; }
        public int TutorialDuration { get; set; }

    }
}
