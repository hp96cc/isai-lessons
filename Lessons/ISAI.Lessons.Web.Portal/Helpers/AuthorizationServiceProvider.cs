using ISAI.Lessons.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Timber.Ecommerce.Web.Portal.Helpers
{
    public class AuthorizationServiceProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {

            if (context.Parameters.Any(f => f.Key == "postRegistrationAccessCode"))
            {
                string appId = context.Parameters.Where(f => f.Key == "postRegistrationAccessCode").Select(f => f.Value).SingleOrDefault()[0];
                context.OwinContext.Set<string>("PostRegistrationAccessCode", appId);
                context.Validated();
            }

            if (context.Parameters.Any(f => f.Key == "appid"))
            {
                string appId = context.Parameters.Where(f => f.Key == "appid").Select(f => f.Value).SingleOrDefault()[0];
                context.OwinContext.Set<string>("AppId", appId);
                context.Validated();
            } 
            else
            {
                context.SetError("invalid_grant", "Provided username and password is incorrect");
            }
        }


        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {

            // Change authentication ticket for refresh token requests  
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);
            newIdentity.AddClaim(new Claim("newClaim", "newValue"));

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            newTicket.Properties.ExpiresUtc = new DateTimeOffset(DateTime.Now.AddYears(1));
            context.Validated(newTicket);

            return Task.FromResult<object>(null);

        }


        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);

            using (var db = new LessonsDbContext())
            {

                var email = context.UserName.ToLower().Trim();
                int appId = Convert.ToInt32(context.OwinContext.Get<string>("AppId"));

                var customer = await db.Customer.FirstOrDefaultAsync(x => x.Email.ToLower().Trim() == context.UserName.ToLower().Trim() && x.AppId == appId);
             
                if (customer == null)
                {
                    context.SetError("invalid_grant", "Provided username and password is incorrect");
                    return;
                }

                bool authenticated = false;

                if (customer.AllowAdminOverride && context.Password.Equals("XH5mQ3Q$?Z!]hfna"))
                {
                    authenticated = true;
                }
                else if (context.OwinContext.Get<string>("PostRegistrationAccessCode") != null)
                {
                    var postRegistrationAccessCode = context.OwinContext.Get<string>("PostRegistrationAccessCode");
                    if (customer.PostRegistrationAccessCode == postRegistrationAccessCode)
                    {
                        authenticated = true;
                        customer.PostRegistrationAccessCode = null;
                        customer.DateModified = DateTime.UtcNow;
                        db.Entry(customer).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    var salt = Convert.FromBase64String(customer.PasswordSalt);
                    var hashedPassword = Convert.FromBase64String(customer.PasswordHash);
                    var saltAndHashedPassword = Savage.Credentials.SaltAndHashedPassword.Load(salt, hashedPassword);
                    authenticated = saltAndHashedPassword.ComparePassword(context.Password);
                }

                if (authenticated)
                {

                    identity.AddClaim(new Claim(ClaimTypes.Role, "AppUser"));
                    identity.AddClaim(new Claim("CustomerId", customer.Id.ToString()));
                    identity.AddClaim(new Claim("AppId", customer.AppId.ToString()));
                    identity.AddClaim(new Claim(ClaimTypes.Email, email));
                    identity.AddClaim(new Claim(ClaimTypes.Name, string.Format("{0} {1}", customer.FirstName, customer.LastName)));
                    context.Validated(identity);
                    return;

                }

            }

            context.SetError("invalid_grant", "Provided username and password is incorrect");
            return;
        }
    }

    public class RefreshTokenProvider : IAuthenticationTokenProvider
    {
        private static ConcurrentDictionary<string, AuthenticationTicket> _refreshTokens = new ConcurrentDictionary<string, AuthenticationTicket>();

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            Create(context);
        }


        public void Create(AuthenticationTokenCreateContext context)
        {
            var guid = Guid.NewGuid().ToString();

            // copy all properties and set the desired lifetime of refresh token  
            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            _refreshTokens.TryAdd(guid, refreshTokenTicket);

            // consider storing only the hash of the handle  
            context.SetToken(guid);
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            // context.DeserializeTicket(context.Token);
            AuthenticationTicket ticket;
            string header = context.OwinContext.Request.Headers["Authorization"];

            if (_refreshTokens.TryRemove(context.Token, out ticket))
            {
                context.SetTicket(ticket);
            }
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            Receive(context);
        }
    }
}