using ISAI.Lessons.Mobile.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LessonGroupPage : ContentPage
    {
        LessonGroupViewModel _viewModel;

        public LessonGroupPage(int? parentId  = null)
        {
            InitializeComponent();
            BindingContext = _viewModel = new LessonGroupViewModel(parentId);
        }

        public LessonGroupPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new LessonGroupViewModel(null);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();

        }


    }
}