using Hangfire.Dashboard;
using Microsoft.Owin;

namespace ISAI.Lessons.Web.Portal.Helpers
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var owinContext = new OwinContext(context.GetOwinEnvironment());

            if (owinContext.Authentication.User.Identity.IsAuthenticated)
            {
                var email = owinContext.Authentication.User.Identity.GetClaim("Email").ToLower();

                if (email == "support@isai.co.uk")
                {
                    return true;
                }
            }

            return false;

        }
    }

}