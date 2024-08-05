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

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
            NavigationPage.SetHasNavigationBar(this, false);
            DependencyService.Get<IDeviceOrientation>().LockOrientation(DeviceOrientations.Landscape);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisappearing();
            NavigationPage.SetHasNavigationBar(this, true);
            DependencyService.Get<IDeviceOrientation>().UnlockOrientation();
        }

        private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
        {
            _viewModel.OnVideoViewInitialized();
        }

    }
}


