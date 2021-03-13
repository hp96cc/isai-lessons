using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class User : IdentityUser
    {

        public User()
        {
            DateCreated = DateTime.UtcNow;
            DateModified = DateTime.UtcNow;
            ModifiedBy = "Sys Admin";

        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            userIdentity.AddClaim(new Claim("Id", this.Id.ToString()));
            userIdentity.AddClaim(new Claim("Firstname", this.Firstname == null ? "" : this.Firstname.ToString()));
            userIdentity.AddClaim(new Claim("Surname", this.Surname == null ? "" : this.Surname.ToString()));
            userIdentity.AddClaim(new Claim("Email", this.Email.ToString()));
            userIdentity.AddClaim(new Claim("Position", this.Position == null ? "" : this.Position.ToString()));
            userIdentity.AddClaim(new Claim("Role", this.Role == null ? "" : this.Role.ToString()));
            userIdentity.AddClaim(new Claim("IsSalesUser", this.IsSalesUser ? "1" : "0"));

            return userIdentity;
        }

        public string Fullname { get; set; }

        public string Firstname { get; set; }

        public string Surname { get; set; }

        public bool IsSalesUser { get; set; }

        public string Position { get; set; }

        public string Role { get; set; }

        public bool Deleted { get; set; }


        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string ModifiedBy { get; set; }

        public string CreatedBy { get; set; }

      

    }

}