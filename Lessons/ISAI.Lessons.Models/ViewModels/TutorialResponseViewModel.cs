using System.Collections.Generic;

namespace ISAI.Lessons.Models.ViewModels
{
    public class TutorialResponseViewModel
    {
        public List<Tutorial> Tutorials { get; set; }
    }

    public class Tutorial
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsCompleted { get; set; }

        public string TeamsLink { get; set; }

        public string Date { get; set; }

        public string TimeStart { get; set; }

        public string TimeEnd { get; set; }

        public string TutorName { get; set; }

    }

}
