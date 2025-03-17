namespace ISAI.Lessons.Models.Interfaces
{
    public interface IGroupTutorialCustomer : IBaseInterface
    {
        int GroupTutorialId { get; set; }

        int CustomerId { get; set; }
    }
}