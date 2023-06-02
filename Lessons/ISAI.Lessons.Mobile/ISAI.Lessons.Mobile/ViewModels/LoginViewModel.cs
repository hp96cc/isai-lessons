using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using Microsoft.AppCenter.Crashes;
//TODO IMPORT: using Plugin.Hud;
//TODO IMPORT: using Plugin.Hud.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace ISAI.Lessons.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {

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
        }

        public void OnAppearing()
        {
      
        }




        async Task OnLoginClicked()
        {
            //TODO IMPORT: CrossHud.Current.Show("Signing in...", -1, MaskType.Black);

            await Task.Delay(100);

            try
            {

                var apiService = new ApiService(false, DependencyService.Get<IAuthService>());
                var loginResponse = await apiService.Login(new EntityFramework.ViewModels.LoginRequestViewModel()
                {
                    Email = _email,
                    Password = _password
                });

                if (loginResponse.Status == ResponseStatus.OK)
                {

                    var appUser = new AppUser()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = _email,
                        Password = _password,
                        AccessToken = loginResponse.Content.AccessToken,
                        RefreshToken = loginResponse.Content.RefreshToken,
                        MaxFileDownloads = 3
                    };

                    await DependencyService.Get<ISqliteService>().SaveUserAsync(appUser);

                    //TODO IMPORT: CrossHud.Current.Show("Downloading Lesson Data...", -1, MaskType.Black);
                    await DownloadLessonData();
                    //TODO IMPORT: CrossHud.Current.Dismiss();

                    var readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                    var writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

                    await Shell.Current.GoToAsync($"//{nameof(AboutPage)}", true);

                    return;
                } 
              

            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                Debug.WriteLine(ex.StackTrace);

            }

            //TODO IMPORT: CrossHud.Current.Dismiss();
            //TODO IMPORT: CrossHud.Current.ShowError("Could not login. Please check your email and / or password. ", MaskType.Black, TimeSpan.FromSeconds(3));

        }


        async Task OnForgotPasswordClicked()
        {
            await Browser.OpenAsync("https://scottishonlinelessons.com/forgot-password", BrowserLaunchMode.External);
        }


        async Task DownloadLessonData()
        {

            var apiService = new ApiService(false, DependencyService.Get<IAuthService>());
            var lessonGroupResponse = await apiService.GetLessonGroupsAsync();

            if (lessonGroupResponse.Status == ResponseStatus.OK)
            {
                await DependencyService.Get<ISqliteService>().SaveLessonGroupsAsync(lessonGroupResponse.Content);
            }
            else
            {
                //TODO IMPORT:  CrossHud.Current.Dismiss();
                //TODO IMPORT: CrossHud.Current.ShowError(lessonGroupResponse.ErrorResponse[0].Message, MaskType.Black, TimeSpan.FromSeconds(3));
                await Task.Delay(3000);
                throw new Exception("Could not download Lesson Groups");
            }

            var lessonResponse = await apiService.GetLessonsAsync();

            if (lessonResponse.Status == ResponseStatus.OK)
            {
                await DependencyService.Get<ISqliteService>().SaveLessonsAsync(lessonResponse.Content);
            }
            else
            {
                //TODO IMPORT:  CrossHud.Current.Dismiss();
                //TODO IMPORT: CrossHud.Current.ShowError(lessonResponse.ErrorResponse[0].Message, MaskType.Black, TimeSpan.FromSeconds(3));
                await Task.Delay(3000);
                throw new Exception("Could not download Lessons");
            }

        
        }
    }
}
