using ISAI.Lessons.Core.Services;
using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Interfaces;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using ISAI.Lessons.Mobile.Services;
using Newtonsoft.Json;
using System;
using Microsoft.Maui.ApplicationModel;

namespace ISAI.Lessons.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH9edHRWRWlZUEx2V0tWYEg=");

            InitializeComponent();

            DependencyService.Register<ISqliteService, SqliteService>();
            DependencyService.RegisterSingleton<IAuthService>(new AuthService());

            Application.Current.UserAppTheme = AppTheme.Light;

            Console.Write("ISAI: App Loaded");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        

    }

}
