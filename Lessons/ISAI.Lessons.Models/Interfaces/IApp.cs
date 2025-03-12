namespace ISAI.Lessons.Models.Interfaces
{
    public interface IApp : IBaseInterface
    {
        string Currency { get; set; }
        string Name { get; set; }
        string StripeLivePublishApiKey { get; set; }
        string StripeLiveSecretApiKey { get; set; }
        string StripeTestPublishApiKey { get; set; }
        string StripeTestSecretApiKey { get; set; }
        bool StripeUseSandbox { get; set; }
        decimal TutorialCost20Minutes { get; set; }
        decimal TutorialCost40Minutes { get; set; }
        decimal TutorialCost60Minutes { get; set; }

        string TutorialStripPriceId20Minutes { get; set; }
        string TutorialStripPriceId40Minutes { get; set; }
        string TutorialStripPriceId60Minutes { get; set; }

        decimal TutorialStripApplicationFee20Minutes { get; set; }
        decimal TutorialStripApplicationFee40Minutes { get; set; }
        decimal TutorialStripApplicationFee60Minutes { get; set; }
    }
}