using ISAI.Lessons.Core.Services;
using ISAI.Lessons.Mobile.Services;
using ISAI.Lessons.Models.Interfaces;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ISAI.Lessons.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<ISqliteService, SqliteService>();
            DependencyService.RegisterSingleton<IAuthService>(new AuthService());

            MainPage = new AppShell();
        }

        protected  override async void OnStart()
        {
            await DependencyService.Get<ISqliteService>().InitializeAsync();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
