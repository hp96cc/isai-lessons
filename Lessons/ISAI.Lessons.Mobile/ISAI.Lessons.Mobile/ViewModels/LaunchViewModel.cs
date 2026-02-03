using ISAI.Lessons.Mobile.Services;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Interfaces.App;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

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

            await LocalAssetInstaller.EnsureAssetsCopiedAsync();

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
