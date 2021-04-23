using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.Models.Models
{
    public class App : BaseModel
    {

        public string Name { get; set; }

        public string Currency { get; set; }

        public bool StripeUseSandbox { get; set; }

        public string StripeTestPublishApiKey { get; set; }

        public string StripeTestSecretApiKey { get; set; }

        public string StripeLivePublishApiKey { get; set; }

        public string StripeLiveSecretApiKey { get; set; }


    }

}
