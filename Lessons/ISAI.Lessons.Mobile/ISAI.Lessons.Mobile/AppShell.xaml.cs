using ISAI.Lessons.Mobile.ViewModels;
using ISAI.Lessons.Mobile.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(LessonsPage), typeof(LessonsPage));
            Routing.RegisterRoute(nameof(LessonDownloadsPage), typeof(LessonDownloadsPage));
            Routing.RegisterRoute(nameof(LessonGroupPage), typeof(LessonGroupPage));

        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            //await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
