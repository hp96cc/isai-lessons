using ISAI.Lessons.Mobile.ViewModels;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.ViewModels
{
    public class LaunchViewModel : BaseViewModel
    {

        public LaunchViewModel()
        {

        }

        public async Task OnAppearing()
        {
            var appUser = await DependencyService.Get<ISqliteService>().GetUserAsync();

            if (appUser != null)
            {
                await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
        }


    }


}
