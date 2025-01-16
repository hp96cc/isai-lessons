namespace ISAI.Lessons.Models.Interfaces
{
    public interface ILessonGroup : IBaseInterface
    {
        int AppId { get; set; }
        bool HasSubGroups { get; set; }
        int ListOrder { get; set; }
        string Name { get; set; }
        int? ParentLessonGroupId { get; set; }
        int? SubscriptionTypeId { get; set; }

        int? TutorialSubjectGroupId { get; set; }
    }
}