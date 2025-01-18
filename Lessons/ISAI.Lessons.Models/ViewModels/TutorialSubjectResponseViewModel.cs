using System.Collections.Generic;

namespace ISAI.Lessons.Models.ViewModels
{
    public class TutorialSubjectResponseViewModel
    {
        public List<TutorialSubject> TutorialSubjects { get; set; }
    }

    public class TutorialSubject
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

}
