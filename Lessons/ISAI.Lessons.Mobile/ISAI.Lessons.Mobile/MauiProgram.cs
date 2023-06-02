using CommunityToolkit.Maui;

namespace ISAI.Lessons.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .UseMauiApp<App>();

        return builder.Build();
    }
}
