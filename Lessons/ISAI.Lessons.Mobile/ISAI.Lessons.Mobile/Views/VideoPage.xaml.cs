using EmbedIO;
using ISAI.Lessons.Mobile.Platforms;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Models.App;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPage : ContentPage
    {
        private const string _baseUrl = "http://localhost:9696/";
        private const string _sourceUrl = "https://portal.scottishonlinelessons.com";
        private Lesson _lesson;
        private WebServer server;
        private string _streamingUrl;

        public VideoPage(Lesson lesson, string streamingUrl)
        {
            InitializeComponent();
            
            _lesson = lesson;
            _streamingUrl = streamingUrl;

            server = new WebServer(o => o
                    .WithUrlPrefix(_baseUrl)
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
                handler.PlatformView.SetWebChromeClient(new WebPlayerChromeClient(handler));

#elif IOS
#endif
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Title = _lesson.Name;
            DependencyService.Get<IDeviceOrientation>().LockOrientation(DeviceOrientations.Landscape);

            // Load video asynchronously
            await LoadVideoAsync();
        }

        private async Task LoadVideoAsync()
        {
            try
            {

             
                // Build video URL on UI thread (since it's fast)
                var encodedSrc = Uri.EscapeDataString(_streamingUrl ?? string.Empty);

                // Build poster URL
                var posterUrl = string.Format("{0}/VideoHandler.ashx?lessonThumbId={1}", _sourceUrl, _lesson.Id);
                var encodedPoster = Uri.EscapeDataString(posterUrl);

                // Build subtitle URLs
                var subtitleParams = string.Empty;

                if (_lesson.IsSubtitlesAvailable)
                {
                    var englishSubUrl = string.Format("{0}/subtitles/{1}/{1}.vtt", _sourceUrl, _lesson.Id);
                    subtitleParams += "&enSub=" + Uri.EscapeDataString(englishSubUrl);
                }
                if (_lesson.IsSigndSubtitlesAvailable)
                {
                    var sslSubUrl = string.Format("{0}/subtitles/{1}/{1}_signed.vtt", _sourceUrl, _lesson.Id);
                    subtitleParams += "&sslSub=" + Uri.EscapeDataString(sslSubUrl);
                }

                var videoUrl = _baseUrl + "wwwroot/player.html?src=" + encodedSrc + "&poster=" + encodedPoster + subtitleParams;
                VideoView.Source = videoUrl;

            }
            catch (Exception ex)
            {
                // Optionally show error to user
                await DisplayAlert("Error", "Failed to load video", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            server = null;
            
            base.OnDisappearing();
            DependencyService.Get<IDeviceOrientation>().UnlockOrientation();
        }

    }
}


