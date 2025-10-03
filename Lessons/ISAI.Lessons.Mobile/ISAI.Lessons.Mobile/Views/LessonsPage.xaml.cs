using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Models.App;
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
            ItemsListView.SelectionChanged += ItemsListView_SelectionChanged;
        }


        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await DisplayLessons();

        }


        private async void ItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsListView.SelectedItem != null)
            {

                var selectedItem = ItemsListView.SelectedItem as Lesson;
                var lessonPage = new LessonPage(selectedItem.Id);
                await Shell.Current.Navigation.PushAsync(lessonPage, true);

            }

            ItemsListView.SelectedItem = null;


        }

        async Task DisplayLessons()
        {
            Title = Title = (await DependencyService.Get<ISqliteService>().GetLessonGroupAsync(_lessonGroupId)).Name;

            var lessons = (await DependencyService.Get<ISqliteService>().GetLessonsAsync(_lessonGroupId))
                       .OrderBy(x => x.ListOrder)
            .ToList();

            ItemsListView.ItemsSource = lessons;

        }



    }
}


