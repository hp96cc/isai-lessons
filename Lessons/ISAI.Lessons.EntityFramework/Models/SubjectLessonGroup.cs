using ISAI.Lessons.EntityFramework.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class SubjectLessonGroup : BaseModel
    {
        public int LessonGroupId { get; set; }
        [ForeignKey("LessonGroupId")]
        public LessonGroup LessonGroup { get; set; }

        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

    }
}
