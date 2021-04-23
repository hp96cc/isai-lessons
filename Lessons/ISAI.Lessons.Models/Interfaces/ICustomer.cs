using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface ICustomer : IBaseInterface
    {
        int AppId { get; set; }

        string FirstName { get; set; }

        string LastName { get; set; }

        string CompanyName { get; set; }

        string Email { get; set; }

        string Telephone { get; set; }

        bool AcceptMarketing { get; set; }

        bool HasCompletedCheckout { get; set; }

        string PostRegistrationAccessCode { get; set; }

        string StripeCustomerId { get; set; }



        string PaymentSessionId { get; set; }


        string PasswordSalt { get; set; }


        string PasswordHash { get; set; }

        string Password { get; set; }

        string PasswordConfirm { get; set; }
    }
}
