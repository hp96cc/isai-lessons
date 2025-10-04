using ISAI.Lessons.Mobile.ViewModels;
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
    public partial class LessonGroupPage : ContentPage
    {

        int? _parentId = null;

        public LessonGroupPage(int? parentId = null)
        {
            InitializeComponent();
            _parentId = parentId;
            SfItemsListView.SelectionChanged += SfItemsListView_SelectionChanged;

        }


        public LessonGroupPage()
        {
            InitializeComponent();
            SfItemsListView.SelectionChanged += SfItemsListView_SelectionChanged;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(async () => await DisplayLessonGroups());

        }

        private async void SfItemsListView_SelectionChanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
        {
            if (SfItemsListView.SelectedItem != null)
            {

                var selectedItem = SfItemsListView.SelectedItem as LessonGroup;
                

                if (selectedItem.HasSubGroups)
                {
                    var lessonGroupPage = new LessonGroupPage(selectedItem.Id);
                    await Shell.Current.Navigation.PushAsync(lessonGroupPage, true);
                }
                else
                {
                    var lessonsPage = new LessonsPage(selectedItem.Id);
                    await Shell.Current.Navigation.PushAsync(lessonsPage, true);
                }

            }

            SfItemsListView.SelectedItem = null;


        }

        async Task DisplayLessonGroups()
        {

            string title = "Lessons";
            if (_parentId.HasValue)
                Title = (await DependencyService.Get<ISqliteService>().GetLessonGroupAsync(_parentId.Value)).Name;

            var lessonGroups = (await DependencyService.Get<ISqliteService>().GetLessonGroupsAsync(_parentId))
                .OrderBy(x => x.ParentLessonGroupId)
                        .ThenBy(x => x.ListOrder)
                        .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Title = title;
                SfItemsListView.ItemsSource = lessonGroups;
                SfItemsListView.RefreshView();
            });

        }



    }
}