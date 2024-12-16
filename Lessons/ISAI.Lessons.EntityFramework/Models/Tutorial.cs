using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class Tutorial : BaseModel
    {
        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        public string TutorUserId { get; set; }

        [ForeignKey("TutorUserId")]
        public User TutorUser { get; set; }

        public int? LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        public string Name { get; set; }

        public DateTimeOffset DateTimeStart { get; set; }

        public DateTimeOffset DateTimeEnd { get; set; }

        public int DurationInMinutes { get; set; }

        public string TeamsId { get; set; }

        public string TeamsLink { get; set; }


    }
}
