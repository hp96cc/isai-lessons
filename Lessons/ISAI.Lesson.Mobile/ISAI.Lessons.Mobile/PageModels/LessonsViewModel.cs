using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using System.Collections.ObjectModel;

namespace ISAI.Lessons.Mobile.ViewModels
{

    public class LessonsViewModel : BaseViewModel
    {

        int _lessonGroupId;
        bool _hasAppeared;

        Lesson _selectedItem;
        public ObservableCollection<Lesson> Items { get; }
       
        public Command<Lesson> ItemTapped { get; }

        public LessonsViewModel(int lessonGroupId)
        {
            _lessonGroupId = lessonGroupId;
            Items = new ObservableCollection<Lesson>();
            ItemTapped = new Command<Lesson>(OnItemSelected);

        }


        async Task DisplayLessons()
        {

            if (_hasAppeared) return;
            _hasAppeared = true;

            Items.Clear();

            var lessons = (await DependencyService.Get<ISqliteService>().GetLessonsAsync(_lessonGroupId))
                        .OrderBy(x => x.ListOrder)
                        .ToList();

            foreach (var item in lessons)
            {
                if (item.LessonGroupId == _lessonGroupId)
                {
                    Items.Add(item);
                }
            }
        }


        public async void OnAppearing()
        {
            Title = Title = (await DependencyService.Get<ISqliteService>().GetLessonGroupAsync(_lessonGroupId)).Name;
            SelectedItem = null;
            await DisplayLessons();
        }

        public Lesson SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }


        async void OnItemSelected(Lesson item)
        {
            if (item == null)
                return;

            _selectedItem = item;

            var lessonPage = new LessonPage(_selectedItem.Id);
            await Shell.Current.Navigation.PushAsync(lessonPage, true);

        }
    }
}