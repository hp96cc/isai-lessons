namespace ISAI.Lessons.Mobile;

using ISAI.Lessons.Core.Services;
using ISAI.Lessons.Models.Interfaces;

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
    
    protected override async void OnStart()
    {
        await DependencyService.Get<ISqliteService>().InitializeAsync();

    }
}