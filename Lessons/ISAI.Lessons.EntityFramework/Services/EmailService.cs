using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
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

        public static string GetTemplateHTML(string url)
        {

            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        return content.ReadAsStringAsync().Result;
                    }
                }
            }


        }


    }

}