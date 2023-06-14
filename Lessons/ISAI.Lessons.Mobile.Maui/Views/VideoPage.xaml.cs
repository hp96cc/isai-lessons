using ISAI.Lessons.Mobile.ViewModels;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;

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
            DependencyService.Get<IDeviceOrientation>().LockOrientation(DeviceOrientations.Landscape);

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisappearing();

            MediaElement.Stop();
            DependencyService.Get<IDeviceOrientation>().UnlockOrientation();

        }

        void OnMediaOpened(object sender, EventArgs e)
        {
            Console.WriteLine("Media opened.");
        }

        void OnMediaFailed(object sender, EventArgs e)
        {
            Console.WriteLine("Media failed.");
        }

        void OnMediaEnded(object sender, EventArgs e)
        {
            Console.WriteLine("Media ended.");
        }

        void OnSeekCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("Seek completed.");
        }



    }
}


