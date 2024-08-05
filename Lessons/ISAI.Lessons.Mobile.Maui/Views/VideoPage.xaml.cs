using ISAI.Lessons.Mobile.ViewModels;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using LibVLCSharp.Shared;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPage : ContentPage
    {
        VideoViewModel _viewModel;

        public VideoPage(int lessonId, string streamingUrl)
        {
            InitializeComponent();
            BindingContext = _viewModel = new VideoViewModel(lessonId, streamingUrl);

            //streamingUrl = "https://portal.scottishonlinelessons.com/" + streamingUrl + "&isApp=true";

            //var libVLC = new LibVLCSharp.Shared.LibVLC(enableDebugLogs: true);
            //var media = new Media(libVLC, new Uri("https://test-videos.co.uk/vids/bigbuckbunny/mp4/h264/1080/Big_Buck_Bunny_1080_10s_1MB.mp4"));
            //VideoView.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(media) { EnableHardwareDecoding = true }; 
           
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
            DependencyService.Get<IDeviceOrientation>().LockOrientation(DeviceOrientations.Landscape);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisappearing();
            DependencyService.Get<IDeviceOrientation>().UnlockOrientation();
        }

        private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
        {
            _viewModel.OnVideoViewInitialized();
        }

    }
}


