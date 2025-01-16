namespace ISAI.Lessons.AppModels.Models
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
