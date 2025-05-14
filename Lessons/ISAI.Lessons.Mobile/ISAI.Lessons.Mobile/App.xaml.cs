using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebApi;
using ISAI.Lessons.Core.Services;
using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Interfaces;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using ISAI.Lessons.Mobile.Services;
using Microsoft.Maui.Storage;

namespace ISAI.Lessons.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<ISqliteService, SqliteService>();
            DependencyService.RegisterSingleton<IAuthService>(new AuthService());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        

    }

}
