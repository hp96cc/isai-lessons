using Newtonsoft.Json;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class StripeCustomerPortalRequest
    {

        [JsonProperty("returnUrl")]
        public string ReturnUrl { get; set; }
    }
}
