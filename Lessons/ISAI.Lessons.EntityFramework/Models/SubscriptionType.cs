using ISAI.Lessons.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class SubscriptionType : BaseModel, ISubscriptionType
    {
        public string Name { get; set; }

        public string StripePriceId { get; set; }

        public int SubscriptionLengthInMonths { get; set; }

        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }


    }
}
