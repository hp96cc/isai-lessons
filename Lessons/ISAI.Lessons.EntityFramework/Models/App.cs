using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class App : BaseModel, IApp
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
