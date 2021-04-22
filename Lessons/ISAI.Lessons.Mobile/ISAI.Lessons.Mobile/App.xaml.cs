using ISAI.Lessons.Core.Services;
using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.Mobile.Services;
using ISAI.Lessons.Mobile.Views;
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

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<ISqliteService, SqliteService>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
