using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class SubjectTutorUser : BaseModel
    {
        public int SubjectId {  get; set; }
        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

        public string TutorUserId { get; set; }

        [ForeignKey("TutorUserId")]
        public User TutorUser { get; set; }
    }
}
