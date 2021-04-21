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
            Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
            Routing.RegisterRoute(nameof(LessonsPage), typeof(LessonsPage));

        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            //await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
