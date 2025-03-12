using ISAI.Lessons.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class LessonGroup : BaseModel, ILessonGroup
    {
        public string Name { get; set; }

        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }

        public int TutorialSubjectId { get; set; }

        [ForeignKey("TutorialSubjectId")]
        public TutorialSubject TutorialSubject { get; set; }

        public int? ParentLessonGroupId { get; set; }

        [ForeignKey("ParentLessonGroupId")]
        public LessonGroup ParentLessonGroup { get; set; }

        public int? SubscriptionTypeId { get; set; }

        [ForeignKey("SubscriptionTypeId")]
        public SubscriptionType SubscriptionType { get; set; }

        public int ListOrder { get; set; }

        public bool HasSubGroups { get; set; }




    }
}
