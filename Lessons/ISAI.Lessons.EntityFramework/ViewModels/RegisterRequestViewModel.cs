using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class RegisterRequestViewModel 
    {

        [JsonProperty("appId")]
        public int AppId { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("passwordConfirm")]
        public string PasswordConfirm { get; set; }

        [JsonProperty("acceptMarketing")]
        public bool AcceptMarketing { get; set; }


    }
}
