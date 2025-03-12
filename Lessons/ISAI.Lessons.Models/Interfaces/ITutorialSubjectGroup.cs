namespace ISAI.Lessons.Models.Interfaces
{
    public interface ITutorialSubjectGroup : IBaseInterface
    {
        int AppId { get; set; }
        string Name { get; set; }
    }
}