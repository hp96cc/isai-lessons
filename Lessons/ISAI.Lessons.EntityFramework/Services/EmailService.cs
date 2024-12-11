using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace ISAI.Lessons.EntityFramework.Services
{
    public static class EmailService
    {
        public static async Task SetPasswordResetEmail(IdentityMessage message)
        {
                var graphApi = new MicrosoftGraphApiService();
                await graphApi.SendEmail("noreply@scottishonlinelessons.com", message.Subject, message.Body, new List<string>() { message.Destination }, null, new List<string>() { ConfigurationManager.AppSettings["SysAdminEmail"] }, new List<string>() { "noreply@scottishonlinelessons.com" }, true, null);
        }


    }

}