using ISAI.Lessons.Mobile.ViewModels;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

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