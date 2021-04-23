using ISAI.Lessons.Mobile.ViewModels;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Interfaces;
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

        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await DependencyService.Get<ISqliteService>().DeleteDatabaseAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
