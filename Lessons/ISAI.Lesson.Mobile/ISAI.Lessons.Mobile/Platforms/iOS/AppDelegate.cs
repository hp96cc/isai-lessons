using Foundation;
using ISAI.Lessons.Mobile.Maui.Platforms.iOS.Helpers;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Mobile.Helper;

namespace ISAI.Lessons.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    
    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        DependencyService.Register<IHud, Hud>();
        return base.FinishedLaunching(app, options);
    }
    
}