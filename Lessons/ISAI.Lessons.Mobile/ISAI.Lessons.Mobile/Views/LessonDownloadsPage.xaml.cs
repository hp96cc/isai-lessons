using ISAI.Lessons.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LessonDownloadsPage : ContentPage
    {
        LessonDownloadsViewModel _viewModel;

        public LessonDownloadsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new LessonDownloadsViewModel();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();

        }


    }
}