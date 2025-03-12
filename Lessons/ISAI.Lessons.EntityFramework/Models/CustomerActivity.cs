using ISAI.Lessons.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class CustomerActivity : BaseModel, ICustomerActivity
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
