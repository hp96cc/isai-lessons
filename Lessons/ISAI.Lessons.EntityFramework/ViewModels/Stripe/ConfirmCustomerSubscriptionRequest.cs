using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.ViewModels.Stripe
{
    public class ConfirmCustomerSubscriptionRequest
    {

        public int AppId { get; set; }
        public string SessionId { get; set; }

    }
}
