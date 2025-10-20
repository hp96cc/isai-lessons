using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models.App;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using ISAI.Lessons.Models.Enums;

namespace ISAI.Lessons.Mobile.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SetDeviceInfo();
            SetAppInfo();

        }

        private async void Button_Lessons_Clicked(object sender, System.EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(LessonGroupPage)}");
        }

        private async void Button_Downloads_Clicked(object sender, System.EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(LessonDownloadsPage)}");
        }

        private async void Button_Refresh_Clicked(object sender, System.EventArgs e)
        {
            DependencyService.Get<IHud>().ShowSpinner("Downloading Lesson Data...");

            await Task.Delay(100);

            try
            {

                var apiService = new ApiService(DependencyService.Get<IAuthService>(), Constants.BasePortalUrl, Constants.BaseReturnUrl);

                var lessonGroupResponse = await apiService.GetLessonGroupsAsync();

                if (lessonGroupResponse.Status == ResponseStatus.OK)
                {
                    DependencyService.Get<ISqliteService>().SaveLessonGroups(lessonGroupResponse.Content);
                }
                else
                {
                    DependencyService.Get<IHud>().Dismiss();
                    DependencyService.Get<IHud>().ShowError(lessonGroupResponse.ErrorResponse[0].Message, TimeSpan.FromSeconds(3));

                    await Task.Delay(3000);
                    throw new Exception("Could not download Lesson Groups");
                }

                var lessonResponse = await apiService.GetLessonsAsync();

                if (lessonResponse.Status == ResponseStatus.OK)
                {
                    DependencyService.Get<ISqliteService>().SaveLessons(lessonResponse.Content);
                }
                else
                {
                    DependencyService.Get<IHud>().Dismiss();
                    DependencyService.Get<IHud>().ShowError(lessonGroupResponse.ErrorResponse[0].Message, TimeSpan.FromSeconds(3));
                    await Task.Delay(3000);
                    throw new Exception();
                }


                DependencyService.Get<IHud>().Dismiss();


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);

                DependencyService.Get<IHud>().Dismiss();
                DependencyService.Get<IHud>().ShowError("Could not download Lessons. ", TimeSpan.FromSeconds(3));


            }
        }

        private void SetDeviceInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("Device Info: ");
            sb.AppendLine($"Model: {DeviceInfo.Current.Model}");
            sb.AppendLine($"Manufacturer: {DeviceInfo.Current.Manufacturer}");
            sb.AppendLine($"Name: {DeviceInfo.Current.Name}");
            sb.AppendLine($"OS Version: {DeviceInfo.Current.VersionString}");
            sb.AppendLine($"Idiom: {DeviceInfo.Current.Idiom}");
            sb.AppendLine($"Platform: {DeviceInfo.Current.Platform}");

            LabelDeviceData.Text = sb.ToString();
        }

        private void SetAppInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("App Info: ");
            sb.AppendLine($"Version: {AppInfo.Current.VersionString}");
            sb.AppendLine($"Build: {AppInfo.Current.BuildString}");

            LabelAppVersion.Text = sb.ToString();
        }
    }
}