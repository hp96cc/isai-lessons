using System;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface ITutorial : IBaseInterface
    {
        int AppId { get; set; }
        int CustomerId { get; set; }
        DateTimeOffset DateTimeEnd { get; set; }
        DateTimeOffset DateTimeStart { get; set; }
        int DurationInMinutes { get; set; }

        decimal TutorialCost { get; set; }
        bool HasCompletedCheckout { get; set; }
        int? LessonId { get; set; }

        int? TutorialSubjectId { get; set; }
        string Name { get; set; }
        bool PendingEmailConfirmationTutor { get; set; }
        bool PendingEmailConfirmationUser { get; set; }
        string StripePaymentId { get; set; }
        string StripePaymentSessionId { get; set; }
        string TeamsId { get; set; }
        string TeamsLink { get; set; }
        string TutorUserId { get; set; }

        int? GroupTutorialId { get; set; }
    }
}