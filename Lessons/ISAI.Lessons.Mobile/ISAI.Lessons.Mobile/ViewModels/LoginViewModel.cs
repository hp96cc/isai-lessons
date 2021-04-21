using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using Plugin.Hud;
using Plugin.Hud.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            ForgotPasswordCommand = new Command(OnForgotPasswordClicked);
            _apiService = new ApiService();
        }

        public async void OnAppearing()
        {
            if (App.IsLoggedIn)
            {
                await Shell.Current.GoToAsync($"//{nameof(LessonGroupPage)}");
            }

        }

        async void OnLoginClicked(object obj)
        {

            CrossHud.Current.Show("Downloading Lesson Data...", -1, MaskType.Black);
            await DownloadLessonData();
            App.IsLoggedIn = true;
            CrossHud.Current.Dismiss();

           await Shell.Current.GoToAsync($"//{nameof(LessonGroupPage)}", true);
        }


        async void OnForgotPasswordClicked(object obj)
        {
            await Shell.Current.GoToAsync($"{nameof(ForgotPasswordPage)}", true);
        }


        async Task DownloadLessonData()
        {

            try
            {
                var lessonGroupResponse = await _apiService.GetLessonGroupsAsync();
                var lessonResponse = await _apiService.GetLessonsAsync();


                if (lessonGroupResponse.Status == ResponseStatus.OK)
                {
                    App.LessonsGroups = lessonGroupResponse.Content
                        .OrderBy(x => x.ParentLessonGroupId)
                        .ThenBy(x => x.ListOrder)
                        .ToList();

                }
                else
                {
                    //Handle error

                }

                if (lessonResponse.Status == ResponseStatus.OK)
                {
                    App.Lessons = lessonResponse.Content
                        .OrderBy(x => x.ListOrder)
                        .ToList();

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
