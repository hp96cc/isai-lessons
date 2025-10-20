using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Models;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ISAI.Lessons.Mobile.Services
{
    public class AuthService : IAuthService
    {

        public async Task<Auth> GetAuth()
        {
            var user = DependencyService.Get<ISqliteService>().GetUser();

            if (user != null)
            {
                Debug.WriteLine("GET Refresh: {0}", user.RefreshToken);

                return new Auth()
                {
                    Email = user.Email,
                    Password = user.Password,
                    AccessToken = user.AccessToken,
                    RefreshToken = user.RefreshToken
                };



            }
            else
            {
                return null;
            }
        }

        public async Task SetAuth(Auth auth)
        {
            var user = DependencyService.Get<ISqliteService>().GetUser();

            if (user != null)
            {
                user.AccessToken = auth.AccessToken;
                user.RefreshToken = auth.RefreshToken;

                DependencyService.Get<ISqliteService>().SaveUser(user);

                Debug.WriteLine("SAVE Refresh: {0}", user.RefreshToken);

            }
        }
    }
}
