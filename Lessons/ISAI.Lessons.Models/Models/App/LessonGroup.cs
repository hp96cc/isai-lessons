using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.Models.App
{
    public class LessonGroup : BaseModel
    {

        public string Name { get; set; }

        public int AppId { get; set; }

        public int? ParentLessonGroupId { get; set; }

        public int ListOrder { get; set; }

        public bool HasSubGroups { get; set; }


    }
}
