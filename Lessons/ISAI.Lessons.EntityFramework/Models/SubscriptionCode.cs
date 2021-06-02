using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class SubscriptionCode : BaseModel
    {

        public string IssuedTo { get; set; }

        public string Code { get; set; }

        public DateTimeOffset ValidFrom { get; set; }

        public DateTimeOffset? ValidTo { get; set; }

        public DateTimeOffset LicenceExpiryDate { get; set; }

        public int LicenceDays { get; set; }

        public DateTimeOffset? UsedDateTime { get; set; }

        [ForeignKey("SubscriptionId")]
        public Subscription Subscription { get; set; }

        public int? SubscriptionId { get; set; }

        [ForeignKey("SubscriptionTypeId")]
        public SubscriptionType SubscriptionType { get; set; }

        public int SubscriptionTypeId { get; set; }


    }
}

