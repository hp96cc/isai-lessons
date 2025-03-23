using ISAI.Lessons.EntityFramework.ViewModels.Stripe;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.ViewModels.Stripe
{
    public class StripeWebHookBase
    {
        public string id { get; set; }


        [JsonProperty("data.object.client_reference_id")]
        public string client_reference_id { get; set; }
       
    }


}
