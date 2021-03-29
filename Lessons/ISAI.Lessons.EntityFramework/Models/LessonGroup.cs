using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class LessonGroup : BaseModel
    {
        
        public string Name { get; set; }

        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }

        public int? ParentLessonGroupId { get; set; }

        [ForeignKey("ParentLessonGroupId")]
        public LessonGroup ParentLessonGroup { get; set; }


    }
}
