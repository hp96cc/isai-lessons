namespace ISAI.Lessons.Models.Interfaces
{
    public interface ITutorialSubjectTutorUser : IBaseInterface
    {
        int TutorialSubjectId { get; set; }
        string TutorUserId { get; set; }
    }
}