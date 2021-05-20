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
           
            BindingContext = _viewModel = new LessonViewModel(lessonId);
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        //void OnMediaOpened(object sender, EventArgs e)
        //{
        //    Console.WriteLine("Media opened.");
        //}

        //void OnMediaFailed(object sender, EventArgs e)
        //{
        //    Console.WriteLine("Media failed.");
        //}

        //void OnMediaEnded(object sender, EventArgs e)
        //{
        //    Console.WriteLine("Media ended.");
        //}

        //void OnSeekCompleted(object sender, EventArgs e)
        //{
        //    Console.WriteLine("Seek completed.");
        //}


    }
}