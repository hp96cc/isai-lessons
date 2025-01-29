using ISAI.Lessons.Mobile.SampleCode.Models;
using ISAI.Lessons.Mobile.SampleCode.PageModels;

namespace ISAI.Lessons.Mobile.SampleCode.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}