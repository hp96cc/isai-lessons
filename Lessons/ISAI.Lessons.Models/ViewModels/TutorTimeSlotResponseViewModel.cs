using System.Collections.Generic;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class TutorTimeSlotResponseViewModel
    {
        public List<TutorialTimeSlot> TutorialTimeSlots { get; set; }
    }

    public class TutorialTimeSlot
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
