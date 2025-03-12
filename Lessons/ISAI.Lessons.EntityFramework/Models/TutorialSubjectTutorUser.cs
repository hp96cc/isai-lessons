using ISAI.Lessons.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class TutorialSubjectTutorUser : BaseModel, ITutorialSubjectTutorUser
    {
        public int TutorialSubjectId { get; set; }

        [ForeignKey("TutorialSubjectId")]
        public TutorialSubject TutorialSubject { get; set; }

        public string TutorUserId { get; set; }

        [InverseProperty("TutorialSubjectTutorUsers")]
        [ForeignKey("TutorUserId")]
        public User TutorUser { get; set; }
    }
}
