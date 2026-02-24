using EmbedIO;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Models.App;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

#if ANDROID
using ISAI.Lessons.Mobile.Platforms;

#elif IOS
#endif

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPage : ContentPage
    {
        private string _sourceUrl = Constants.BaseReturnUrl;
        private Lesson _lesson;
        private WebServer server;
        private string _streamingUrl;

        public VideoPage(Lesson lesson, string streamingUrl)
        {
            InitializeComponent();
            
            _lesson = lesson;
            _streamingUrl = streamingUrl;

            server = new WebServer(o => o
                    .WithUrlPrefix(Constants.LocalServerBaseUrl)
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

                // Ensure local subtitles folder path: {AppDataDirectory}/subtitles/{lessonId}/...
                var localSubtitleDir = Path.Combine(FileSystem.Current.AppDataDirectory, "subtitles", _lesson.Id.ToString());
                if (!Directory.Exists(localSubtitleDir))
                {
                    Directory.CreateDirectory(localSubtitleDir);
                }

                using var http = new HttpClient();

                if (_lesson.IsSubtitlesAvailable)
                {
                    // Remote source path (used to download), relative path used in the player query so it resolves to the local server root
                    var relativeEnglish = $"/subtitles/{_lesson.Id}/{_lesson.Id}.vtt";
                    var remoteEnglish = $"{_sourceUrl}{relativeEnglish}";
                    var localEnglishPath = Path.Combine(localSubtitleDir, $"{_lesson.Id}.vtt");

                    try
                    {
                        var bytes = await http.GetByteArrayAsync(remoteEnglish);
                        await File.WriteAllBytesAsync(localEnglishPath, bytes);
                        subtitleParams += "&enSub=" + Uri.EscapeDataString(relativeEnglish);
                    }
                    catch (Exception)
                    {
                        // Swallow exception so player still loads without this subtitle.
                    }
                }
                if (_lesson.IsSigndSubtitlesAvailable)
                {
                    var relativeSsl = $"/subtitles/{_lesson.Id}/{_lesson.Id}_signed.vtt";
                    var remoteSsl = $"{_sourceUrl}{relativeSsl}";
                    var localSslPath = Path.Combine(localSubtitleDir, $"{_lesson.Id}_signed.vtt");

                    try
                    {
                        var bytes = await http.GetByteArrayAsync(remoteSsl);
                        await File.WriteAllBytesAsync(localSslPath, bytes);
                        subtitleParams += "&sslSub=" + Uri.EscapeDataString(relativeSsl);
                    }
                    catch (Exception)
                    {
                        // Swallow exception so player still loads without this subtitle.
                    }
                }

                var videoUrl = Constants.LocalServerBaseUrl + "wwwroot/player.html?src=" + encodedSrc + "&poster=" + encodedPoster + subtitleParams;
                VideoView.Source = videoUrl;
            }
            catch (Exception)
            {
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


