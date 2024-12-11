using ISAI.Lessons.Mobile.ViewModels;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LessonPage : ContentPage
    {
        LessonViewModel _viewModel;

        public LessonPage(int lessonId)
        {
           
            BindingContext = _viewModel = new LessonViewModel(lessonId);
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();

        }
    }
}