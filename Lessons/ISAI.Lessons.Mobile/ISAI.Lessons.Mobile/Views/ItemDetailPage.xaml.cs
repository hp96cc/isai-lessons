using ISAI.Lessons.Mobile.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}