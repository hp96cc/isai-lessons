using ISAI.Lessons.Core.Services;
using ISAI.Lessons.Models.Interfaces;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;
using ISAI.Lessons.Mobile.Maui.Services;

namespace ISAI.Lessons.Mobile.Maui
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

        protected override async void OnStart()
        {
            await DependencyService.Get<ISqliteService>().InitializeAsync();

            //AppCenter.Start("ios=0965cd30-5ffc-43db-932e-81a80aad6007;" +
            //      "android=9c21c892-f0b6-471a-830b-c361277385c6;",
            //      typeof(Analytics), typeof(Crashes));

        }
    }
}