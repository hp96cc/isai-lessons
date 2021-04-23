using ISAI.Lessons.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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