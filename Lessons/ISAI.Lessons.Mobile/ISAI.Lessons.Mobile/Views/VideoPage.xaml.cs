using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces.App;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPage : ContentPage
    {
        //VideoViewModel _viewModel;
        private int _lessonId;

        public VideoPage(int lessonId, string streamingUrl)
        {
            InitializeComponent();
            _lessonId = lessonId;
            VideoView.Source = "https://portal.scottishonlinelessons.com" + streamingUrl;
            //BindingContext = _viewModel = new VideoViewModel(lessonId, streamingUrl);



        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var db = DependencyService.Get<ISqliteService>();

            var lesson  = await db.GetLessonAsync(_lessonId);

            Title = lesson.Name;
            //_viewModel.OnAppearing();
            //NavigationPage.SetHasNavigationBar(this, false);
            DependencyService.Get<IDeviceOrientation>().LockOrientation(DeviceOrientations.Landscape);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //_viewModel.OnDisappearing();
            //NavigationPage.SetHasNavigationBar(this, true);
            DependencyService.Get<IDeviceOrientation>().UnlockOrientation();
        }

        //private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
        //{
        //    _viewModel.OnVideoViewInitialized();
        //}

    }
}


