using System.Collections.Generic;

namespace ISAI.Lessons.Models.ViewModels
{
    public class TutorTimeSlotResponseViewModel
    {
        public List<TutorialTimeSlot> TutorialTimeSlots { get; set; }
    }

    public class TutorialTimeSlot
    {
        public int StartTimeHours { get; set; }
        public int StartTimeMinutes { get; set; }

        public int EndTimeHours { get; set; }
        public int EndTimeMinutes { get; set; }
    }
}
