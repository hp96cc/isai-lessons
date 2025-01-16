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
    }
}