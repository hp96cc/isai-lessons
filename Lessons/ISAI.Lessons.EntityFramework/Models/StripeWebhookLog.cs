using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class StripeWebhookLog : BaseModel, IStripeWebhookLog
    {

        public string CallBackName { get; set; }

        public string Description { get; set; }

    }
}
