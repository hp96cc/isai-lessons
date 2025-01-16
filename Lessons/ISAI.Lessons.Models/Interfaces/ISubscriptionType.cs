namespace ISAI.Lessons.Models.Interfaces
{
    public interface ISubscriptionType : IBaseInterface
    {
        int AppId { get; set; }
        string Name { get; set; }
    }
}