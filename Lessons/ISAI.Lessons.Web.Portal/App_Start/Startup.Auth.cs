using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using ISAI.Lessons.EntityFramework;
using ISAI.Lessons.Web.Portal.App_Start;
using ISAI.Lessons.EntityFramework.Models;
using Microsoft.Owin.Security.OAuth;
using Timber.Ecommerce.Web.Portal.Helpers;
using Hangfire;
using System.Configuration;
using ISAI.Lessons.Web.Portal.Helpers;

[assembly: OwinStartup(typeof(ISAI.Lessons.Web.Portal.Startup))]
namespace ISAI.Lessons.Web.Portal
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.MapSignalR();


            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(LessonsDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
       
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, User>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            var myProvider = new AuthorizationServiceProvider();
            OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60),
                Provider = myProvider,
                RefreshTokenProvider = new RefreshTokenProvider()

            };
            app.UseOAuthAuthorizationServer(options);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            GlobalConfiguration.Configuration.UseSqlServerStorage(ConfigurationManager.ConnectionStrings["Hangfire"].ConnectionString);
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
            app.UseHangfireServer();

            var timeZone = ConfigurationManager.AppSettings["SystemTimeZone"];
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            RecurringJob.AddOrUpdate("SchedulerService.SendPendingTutorialEmails", () => SchedulerService.SendPendingTutorialEmails(), Cron.Minutely, new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });
            RecurringJob.AddOrUpdate("SchedulerService.DeleteAbandonedTutorials", () => SchedulerService.DeleteAbandonedTutorials(), Cron.Minutely, new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });

            RecurringJob.AddOrUpdate("SchedulerService.CreateTeamsMeetingForGroupLessons", () => SchedulerService.CreateTeamsMeetingForGroupLessons(), Cron.Minutely, new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });



        }
    }
}