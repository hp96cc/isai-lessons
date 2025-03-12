using System.Collections.Generic;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface ICustomer : IBaseInterface
    {
        bool AcceptMarketing { get; set; }
        bool AllowAdminOverride { get; set; }
        int AppId { get; set; }
        string CompanyName { get; set; }
        string Email { get; set; }
        string FirstName { get; set; }
        bool HasCompletedCheckout { get; set; }
        string HearAbout { get; set; }
        string LastName { get; set; }
        int MaxDevicesAllowed { get; set; }
        string Password { get; set; }
        string PasswordConfirm { get; set; }
        string PasswordHash { get; set; }
        string PasswordSalt { get; set; }
        string PaymentSessionId { get; set; }
        string PostRegistrationAccessCode { get; set; }
        string StripeCustomerId { get; set; }
        string Telephone { get; set; }
    }
}