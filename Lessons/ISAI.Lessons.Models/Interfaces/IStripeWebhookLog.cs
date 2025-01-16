namespace ISAI.Lessons.Models.Interfaces
{
    public interface IStripeWebhookLog : IBaseInterface
    {
        string CallBackName { get; set; }
        string Description { get; set; }
    }
}