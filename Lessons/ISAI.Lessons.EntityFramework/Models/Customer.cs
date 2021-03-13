using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class Customer : BaseModel
    {

        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CompanyName { get; set; }

        public string Email { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }

        public string Telephone { get; set; }

        public string Mobile { get; set; }

        public bool AcceptMarketing { get; set; }



    }
}
