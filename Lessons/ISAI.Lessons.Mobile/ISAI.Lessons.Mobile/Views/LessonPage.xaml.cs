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
    public partial class LessonPage : ContentPage
    {
        LessonViewModel _viewModel;

        public LessonPage(int lessonId)
        {
            InitializeComponent();
            BindingContext = _viewModel = new LessonViewModel(lessonId);
        }


    }
}