using ISAI.Lessons.Mobile.ViewModels;
using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Models.App;
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
            ItemsListView.SelectionChanged += ItemsListView_SelectionChanged;

        }

        public LessonGroupPage()
        {
            InitializeComponent();
            ItemsListView.SelectionChanged += ItemsListView_SelectionChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await DisplayLessonGroups();
        }

        private async void ItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsListView.SelectedItem != null)
            {

                var selectedItem = ItemsListView.SelectedItem as LessonGroup;
                

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

            ItemsListView.SelectedItem = null;


        }

        async Task DisplayLessonGroups()
        {


            if (_parentId.HasValue)
            {
                Title = (await DependencyService.Get<ISqliteService>().GetLessonGroupAsync(_parentId.Value)).Name;
            }
            else
            {
                Title = "Lessons";
            }


            var lessonGroups = (await DependencyService.Get<ISqliteService>().GetLessonGroupsAsync(_parentId))
                .OrderBy(x => x.ParentLessonGroupId)
                        .ThenBy(x => x.ListOrder)
                        .ToList();

            ItemsListView.ItemsSource = lessonGroups;



        }



    }
}