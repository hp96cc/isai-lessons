using EmbedIO;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces.App;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Storage;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPage : ContentPage
    {
        private int _lessonId;

        private WebServer server;

        public VideoPage(int lessonId, string streamingUrl)
        {
            InitializeComponent();
            
            server = new WebServer(o => o
                    .WithUrlPrefix("http://localhost:9696/")
                    .WithMode(HttpListenerMode.EmbedIO))
                    .WithLocalSessionManager()
                    .WithStaticFolder("/", FileSystem.Current.AppDataDirectory, true);
    
            server.RunAsync();

            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("AllowLocalPlay", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.Settings.JavaScriptEnabled = true;
                handler.PlatformView.Settings.AllowFileAccess = true;
                handler.PlatformView.Settings.AllowFileAccessFromFileURLs = true;
                handler.PlatformView.Settings.AllowUniversalAccessFromFileURLs = true;

#elif IOS
#endif
            });

            

            _lessonId = lessonId;
            //VideoView.Source = "file:///data/user/0/uk.co.isai.uteachlessons.pupil.droid/files/420/420.m3u8";
            VideoView.Source = streamingUrl;

        }
        
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var db = DependencyService.Get<ISqliteService>();

            var lesson  = await db.GetLessonAsync(_lessonId);

            Title = lesson.Name;
            DependencyService.Get<IDeviceOrientation>().LockOrientation(DeviceOrientations.Landscape);
        }

        protected override void OnDisappearing()
        {
            server = null;
            
            base.OnDisappearing();
            DependencyService.Get<IDeviceOrientation>().UnlockOrientation();
        }


    }
}


