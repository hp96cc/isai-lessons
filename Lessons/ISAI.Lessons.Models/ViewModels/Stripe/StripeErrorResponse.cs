using Newtonsoft.Json;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class StripeErrorMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class StripeErrorResponse
    {
        [JsonProperty("error")]
        public StripeErrorMessage StripeErrorMessage { get; set; }
    }
}
