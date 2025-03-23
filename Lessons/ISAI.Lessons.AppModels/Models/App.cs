using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.AppModels.Models
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
        public decimal TutorialCost20Minutes { get; set; }
        public decimal TutorialCost40Minutes { get; set; }
        public decimal TutorialCost60Minutes { get; set; }
        
        public decimal TutorialStripApplicationFee20Minutes { get; set; }
        public decimal TutorialStripApplicationFee40Minutes { get; set; }
        public decimal TutorialStripApplicationFee60Minutes { get; set; }

        public string GroupTutorialStripePriceId { get; set; }
    }

}
