using ISAI.Lessons.Mobile.Views;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ISAI.Lessons.Models.Interfaces.App;

namespace ISAI.Lessons.Mobile.ViewModels
{
    public class LaunchViewModel : BaseViewModel
    {

        public LaunchViewModel()
        {

        }

        public async Task OnAppearing()
        {

            DependencyService.Get<ISqliteService>().Initialize();

            var appUser = DependencyService.Get<ISqliteService>().GetUser();

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
