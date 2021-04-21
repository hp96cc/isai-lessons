using Newtonsoft.Json;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class StripeSetupResponse
    {
        [JsonProperty("publishableKey")]
        public string PublishableKey { get; set; }

        [JsonProperty("proPrice")]
        public string ProPrice { get; set; }

        [JsonProperty("basicPrice")]
        public string BasicPrice { get; set; }
    }
}
