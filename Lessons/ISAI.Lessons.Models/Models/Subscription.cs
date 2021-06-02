using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace ISAI.Lessons.Models.Models
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
