using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.AppModels.Models
{
    public class SubscriptionCode : BaseModel
    {

        public string IssuedTo { get; set; }

        public string Code { get; set; }

        public DateTimeOffset ValidFrom { get; set; }

        public DateTimeOffset? ValidTo { get; set; }

        public DateTimeOffset LicenceExpiryDate { get; set; }

        public int LicenceDays { get; set; }

        public int SubscriptionTypeId { get; set; }
    }
}
