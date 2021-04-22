using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using Plugin.Hud;
using Plugin.Hud.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {

        ApiService _apiService;
        public Command LoginCommand { get; }
        public Command ForgotPasswordCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            ForgotPasswordCommand = new Command(async () => await OnForgotPasswordClicked());
            _apiService = new ApiService();
        }

        public async void OnAppearing()
        {
            if (App.IsLoggedIn)
            {
                await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
            }

        }

        async void OnLoginClicked(object obj)
        {

            var readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
            var writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

            CrossHud.Current.Show("Downloading Lesson Data...", -1, MaskType.Black);
            await DownloadLessonData();
            App.IsLoggedIn = true;
            CrossHud.Current.Dismiss();

           await Shell.Current.GoToAsync($"//{nameof(AboutPage)}", true);
        }


        async Task OnForgotPasswordClicked()
        {
            await Browser.OpenAsync("https://scottishonlinelessons.com/forgotpassword", BrowserLaunchMode.External);
        }


        async Task DownloadLessonData()
        {

            try
            {
                var lessonGroupResponse = await _apiService.GetLessonGroupsAsync();
                var lessonResponse = await _apiService.GetLessonsAsync();


                if (lessonGroupResponse.Status == ResponseStatus.OK)
                {
                    await DependencyService.Get<ISqliteService>().SaveLessonGroupsAsync(lessonGroupResponse.Content);
                }
                else
                {
                    //Handle error

                }

                if (lessonResponse.Status == ResponseStatus.OK)
                {
                    await DependencyService.Get<ISqliteService>().SaveLessonsAsync(lessonResponse.Content);
                }
                else
                {
                    //Handle error

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
             
            }

        }
    }
}
