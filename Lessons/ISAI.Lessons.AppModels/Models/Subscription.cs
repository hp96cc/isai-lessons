using System;

namespace ISAI.Lessons.AppModels.Models
{
    public class Subscription : BaseModel
    {

        public int CustomerId { get; set; }

        public string Name { get; set; }

        public string StripeSubscriptionId { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public bool Active { get; set; }

        public int SubscriptionTypeId { get; set; }

    }
}
