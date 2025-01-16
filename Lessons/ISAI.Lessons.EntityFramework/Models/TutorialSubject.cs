using ISAI.Lessons.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class TutorialSubject : BaseModel, ITutorialSubject
    {
        public int TutorialSubjectGroupId { get; set; }

        [ForeignKey("TutorialSubjectGroupId")]
        public TutorialSubjectGroup TutorialSubjectGroup { get; set; }

        public string Name { get; set; }
 
    }
}
