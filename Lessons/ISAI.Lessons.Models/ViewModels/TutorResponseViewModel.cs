using System;
using System.Collections.Generic;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class TutorResponseViewModel
    {
        public List<Tutor> Tutors { get; set; }
    }

    public class Tutor
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
