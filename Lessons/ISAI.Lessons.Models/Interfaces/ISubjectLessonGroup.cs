namespace ISAI.Lessons.Models.Interfaces
{
    public interface ISubjectLessonGroup : IBaseInterface
    {
        int LessonGroupId { get; set; }
        int SubjectId { get; set; }
    }
}