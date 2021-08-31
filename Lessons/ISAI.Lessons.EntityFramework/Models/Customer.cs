using Newtonsoft.Json;
using ISAI.Lessons.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class Customer : BaseModel, ICustomer
    {

        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CompanyName { get; set; }

        public string Email { get; set; }

        public string Telephone { get; set; }

        public bool AcceptMarketing { get; set; }

        public bool HasCompletedCheckout { get; set; }

        public string PostRegistrationAccessCode { get; set; }

        public string StripeCustomerId { get; set; }

        public int MaxDevicesAllowed { get; set; }

        public string HearAbout { get; set; }

        public bool AllowAdminOverride { get; set; }


        [JsonIgnore]
        public string PaymentSessionId { get; set; }

        [JsonIgnore]
        public string PasswordSalt { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }

        [NotMapped]
        public string Password{ get; set; }

        [NotMapped]
        public string PasswordConfirm { get; set; }

    }
}
