using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.Mobile.Models;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.ViewModels
{

    public class LessonGroupViewModel : BaseViewModel
    {

        public int? ParentId
        {
            get => _parentId;
            set
            {
                SetProperty(ref _parentId, value);
            }
        }
        int? _parentId;



        LessonGroup _selectedItem;
        public ObservableCollection<LessonGroup> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command<LessonGroup> ItemTapped { get; }

        public LessonGroupViewModel(int? parentId)
        {
            Title = string.Empty;
            _parentId = parentId;
            Items = new ObservableCollection<LessonGroup>();
            LoadItemsCommand = new Command(async () => await DisplayLessonGroups());
            ItemTapped = new Command<LessonGroup>(OnItemSelected);

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

            IsBusy = true;

            Items.Clear();

            var lessonGroups = (await DependencyService.Get<ISqliteService>().GetLessonGroupsAsync(_parentId))
                .OrderBy(x => x.ParentLessonGroupId)
                        .ThenBy(x => x.ListOrder)
                        .ToList();

            foreach (var item in lessonGroups)
            {
                if (item.ParentLessonGroupId == _parentId)
                {
                    Items.Add(item);

                }
            }

            IsBusy = false;
        }


        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public LessonGroup SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }


        async void OnItemSelected(LessonGroup item)
        {
            if (item == null)
                return;

            _selectedItem = item;

            if (_selectedItem.HasSubGroups)
            {
                var lessonGroupPage = new LessonGroupPage(_selectedItem.Id);
                await Shell.Current.Navigation.PushAsync(lessonGroupPage, true);
            }
            else
            {
                var lessonsPage = new LessonsPage(_selectedItem.Id);
                await Shell.Current.Navigation.PushAsync(lessonsPage, true);
            }


        }
    }
}