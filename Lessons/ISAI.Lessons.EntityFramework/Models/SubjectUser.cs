using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class SubjectUser : BaseModel
    {
        public int SubjectId {  get; set; }
        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
