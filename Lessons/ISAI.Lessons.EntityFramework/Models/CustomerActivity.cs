using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class CustomerActivity : BaseModel
    {

        public int CustomerDeviceId { get; set; }

        [ForeignKey("CustomerDeviceId")]
        public CustomerDevice CustomerDevice { get; set; }

        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        public DateTimeOffset StartDateTime { get; set; }



    }
}
