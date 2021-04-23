using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
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

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
            }
        }
        string _email;


        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
            }
        }
        string _password;



        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await OnLoginClicked());
            ForgotPasswordCommand = new Command(async () => await OnForgotPasswordClicked());
            _apiService = new ApiService(true);
        }

        public async void OnAppearing()
        {

            //var appUser = await DependencyService.Get<ISqliteService>().GetUserAsync();

            //if (appUser != null)
            //{
            //    await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
            //}

        }

        async Task OnLoginClicked()
        {

            var apiService = new ApiService(true);
            var loginResponse = await apiService.Login(new EntityFramework.ViewModels.LoginRequestViewModel()
            {
                Email = _email,
                Password = _password
            });

            if (loginResponse.Status == ResponseStatus.OK)
            {
                CrossHud.Current.ShowError("Username and / or password are incorrect", MaskType.Black);
            }
            else
            {

                var appUser = new AppUser()
                {
                    Id = Guid.NewGuid(),
                    Email = _email,
                    Password = _password,
                    AccessToken = loginResponse.Content.AccessToken,
                    RefreshToken = loginResponse.Content.RefreshToken,
                    MaxFileDownloads = 3
                };

                await DependencyService.Get<ISqliteService>().SaveUserAsync(appUser);

                var readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                var writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

                CrossHud.Current.Show("Downloading Lesson Data...", -1, MaskType.Black);
                await DownloadLessonData();
                CrossHud.Current.Dismiss();

                await Shell.Current.GoToAsync($"//{nameof(AboutPage)}", true);
            }


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
