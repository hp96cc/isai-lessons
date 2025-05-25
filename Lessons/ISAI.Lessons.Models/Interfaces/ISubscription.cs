using System;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface ISubscription : IBaseInterface
    {
        bool Active { get; set; }
        int CustomerId { get; set; }
        DateTimeOffset EndDate { get; set; }
        string Name { get; set; }
        DateTimeOffset StartDate { get; set; }
        string StripeSubscriptionId { get; set; }
        int SubscriptionTypeId { get; set; }

        bool HasAbondonedSubscriptionEmailBeenSent { get; set; }
    }
}