using ISAI.Lessons.Mobile.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Threading.Tasks;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LaunchPage : ContentPage
    {
        LaunchViewModel _viewModel;

        public LaunchPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new LaunchViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(1000);
            await _viewModel.OnAppearing();
        }
    }
}