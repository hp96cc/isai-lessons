using System.Security.Claims;
using System.Security.Principal;

namespace ISAI.Lessons.Web.Portal.Helpers
{
    public static class IdentityExtensions
    {
        public static string GetClaim(this IIdentity identity, string ClaimName)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst(ClaimName);
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        
    }
}