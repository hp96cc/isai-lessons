using Newtonsoft.Json;
using System.Collections.Generic;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class CreateCustomerPayemntSessionResponse
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        public List<string> Errors { get; set; }

    }
}
