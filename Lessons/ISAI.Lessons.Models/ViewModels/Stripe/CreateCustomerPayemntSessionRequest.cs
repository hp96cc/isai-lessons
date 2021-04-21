using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
