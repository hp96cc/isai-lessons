using ISAI.Lessons.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class TutorialSubjectGroup : BaseModel, ITutorialSubjectGroup
    {
        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }
        public string Name { get; set; }
  
    }
}
