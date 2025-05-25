using ISAI.Lessons.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class Subscription : BaseModel, ISubscription
    {

        public int CustomerId { get; set; }


        public string Name { get; set; }

        public string StripeSubscriptionId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public bool Active { get; set; } = true; //TODO: check

        [ForeignKey("SubscriptionTypeId")]
        public SubscriptionType SubscriptionType { get; set; }

        public int SubscriptionTypeId { get; set; }

        public bool HasAbondonedSubscriptionEmailBeenSent { get; set; }

    }
}
