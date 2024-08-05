using ISAI.Lessons.Models.Models;

namespace ISAI.Lessons.Mobile.ViewModels
{

    public class VideoViewModel : BaseViewModel
    {
        int _lessonId;
        Lesson _lesson;
        string _streamingUrl;

        private LibVLCSharp.Shared.LibVLC LibVLC { get; set; }

        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        public LibVLCSharp.Shared.MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => SetProperty(ref _mediaPlayer, value);
        }

        private bool IsLoaded { get; set; }
        private bool IsVideoViewInitialized { get; set; }
        private string _baseUrl = "https://portal.scottishonlinelessons.com/";


        public VideoViewModel(int lessonId, string streamingUrl)
        {
            _lessonId = lessonId;
            _streamingUrl = _baseUrl + streamingUrl;
            Initialize();

        }

        private void Initialize()
        {
            LibVLC = new LibVLCSharp.Shared.LibVLC(enableDebugLogs: true);
            using var media = new LibVLCSharp.Shared.Media(LibVLC, new Uri(_streamingUrl));

            MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(LibVLC)
            {
                Media = media
            };
        }


        public void OnAppearing()
        {
            IsLoaded = true;
            Play();
        }

        internal void OnDisappearing()
        {
            MediaPlayer.Dispose();
            LibVLC.Dispose();
        }

        public void OnVideoViewInitialized()
        {
            IsVideoViewInitialized = true;
            Play();
        }

        private void Play()
        {
            if (IsLoaded && IsVideoViewInitialized)
            {
                MediaPlayer.Play();
            }
        }
    }
}
