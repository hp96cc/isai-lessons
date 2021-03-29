using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class Lesson : BaseModel
    {
        
        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }

        public int LessonGroupId { get; set; }

        [ForeignKey("LessonGroupId")]
        public LessonGroup LessonGroup { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string LessonNotes { get; set; }

        public int ListOrder { get; set; }

        public string AssetId { get; set; }


    }
}
