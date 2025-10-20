using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Models.App;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Linq;
using System.Threading.Tasks;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LessonsPage : ContentPage
    {


        int _lessonGroupId;

        public LessonsPage(int lessonGroupId)
        {
            InitializeComponent();
            _lessonGroupId = lessonGroupId;
            SfItemsListView.SelectionChanged += SfItemsListView_SelectionChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            DisplayLessons();

        }


        private async void SfItemsListView_SelectionChanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
        {
            if (SfItemsListView.SelectedItem != null)
            {

                var selectedItem = SfItemsListView.SelectedItem as Lesson;
                var lessonPage = new LessonPage(selectedItem.Id);
                await Shell.Current.Navigation.PushAsync(lessonPage, true);

            }

            SfItemsListView.SelectedItem = null;


        }

        void DisplayLessons()
        {
            var title = Title = (DependencyService.Get<ISqliteService>().GetLessonGroup(_lessonGroupId)).Name;

            var lessons = (DependencyService.Get<ISqliteService>().GetLessons(_lessonGroupId))
                       .OrderBy(x => x.ListOrder)
                        .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Title = title;
                SfItemsListView.ItemsSource = lessons;
                SfItemsListView.RefreshView();
            });

        }



    }
}


