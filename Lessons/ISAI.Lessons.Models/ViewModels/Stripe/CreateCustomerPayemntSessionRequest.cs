using Newtonsoft.Json;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class CreateCustomerPayemntSessionRequest : RegisterRequestViewModel
    {

        [JsonProperty("subscriptionTypeId")]
        public int SubscriptionTypeId { get; set; }

        [JsonProperty("successUrl")]
        public string SuccessUrl { get; set; }

        [JsonProperty("cancelUrl")]
        public string CancelUrl { get; set; }


    }
}
