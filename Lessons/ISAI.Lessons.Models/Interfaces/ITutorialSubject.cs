namespace ISAI.Lessons.Models.Interfaces
{
    public interface ITutorialSubject : IBaseInterface
    {
        int TutorialSubjectGroupId { get; set; }
        string Name { get; set; }
    }
}