using ISAI.Lessons.Mobile.ViewModels;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LessonsPage : ContentPage
    {
        LessonsViewModel _viewModel;

        public LessonsPage(int lessonGroupId)
        {
            InitializeComponent();
            BindingContext = _viewModel = new LessonsViewModel(lessonGroupId);
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();

        }


    }
}