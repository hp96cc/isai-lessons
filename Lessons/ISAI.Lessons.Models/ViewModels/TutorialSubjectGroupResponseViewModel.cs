using System.Collections.Generic;

namespace ISAI.Lessons.Models.ViewModels
{
    public class TutorialSubjectGroupResponseViewModel
    {
        public List<TutorialSubjectGroup> TutorialSubjectGroups { get; set; }
    }

    public class TutorialSubjectGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

}
