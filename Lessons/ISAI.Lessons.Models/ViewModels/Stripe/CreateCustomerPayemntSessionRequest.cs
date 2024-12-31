using Newtonsoft.Json;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class CreateCustomerPayemntSessionRequest : RegisterRequestViewModel
    {

        [JsonProperty("priceId")]
        public string PriceId { get; set; }

        [JsonProperty("successUrl")]
        public string SuccessUrl { get; set; }

        [JsonProperty("cancelUrl")]
        public string CancelUrl { get; set; }


    }
}
