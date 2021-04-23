using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.Services
{
    public class AuthService : IAuthService
    {

        public async Task<Auth> GetAuth()
        {
            var user = await DependencyService.Get<ISqliteService>().GetUserAsync();

            if(user != null)
            {
                Debug.WriteLine("GET Refresh: {0}", user.RefreshToken);

                return new Auth()
                {
                    AccessToken = user.AccessToken,
                    RefreshToken = user.RefreshToken
                };

              

            } else
            {
                return null;
            }
        }

        public async Task SetAuth(Auth auth)
        {
            var user = await DependencyService.Get<ISqliteService>().GetUserAsync();

            if (user != null)
            {
                user.AccessToken = auth.AccessToken;
                user.RefreshToken = auth.RefreshToken;

                await DependencyService.Get<ISqliteService>().SaveUserAsync(user);

                Debug.WriteLine("SAVE Refresh: {0}", user.RefreshToken);

            }
        }
    }
}
