using ISAI.Lessons.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class Lesson : BaseModel, ILesson
    {
        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }

        public int LessonGroupId { get; set; }

        [ForeignKey("LessonGroupId")]
        public LessonGroup LessonGroup { get; set; }

        public int? SubscriptionTypeId { get; set; }

        [ForeignKey("SubscriptionTypeId")]
        public SubscriptionType SubscriptionType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string LessonNotes { get; set; }

        public int ListOrder { get; set; }

        public string AssetId { get; set; }

        public string SourceUrl { get; set; }

        public bool PendingDownload { get; set; }

        public string BitmovinId { get; set; }



    }
}
