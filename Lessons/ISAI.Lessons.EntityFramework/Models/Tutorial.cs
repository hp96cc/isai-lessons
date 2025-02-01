using ISAI.Lessons.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class Tutorial : BaseModel, ITutorial
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

        public decimal TutorialCost { get; set; }

        public string Notes { get; set; }

        public string TeamsId { get; set; }

        public string TeamsLink { get; set; }

        [JsonIgnore]
        public string StripePaymentSessionId { get; set; }

        [JsonIgnore]
        public string StripePaymentId { get; set; }

        [JsonIgnore]
        public bool HasCompletedCheckout { get; set; }

        [JsonIgnore]
        public bool PendingEmailConfirmationUser { get; set; }

        [JsonIgnore]
        public bool PendingEmailConfirmationTutor { get; set; }
     
    }
}
