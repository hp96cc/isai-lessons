using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.AppModels.Models
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
