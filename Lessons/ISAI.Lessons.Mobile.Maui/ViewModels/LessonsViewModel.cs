using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.Mobile.Models;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace ISAI.Lessons.Mobile.ViewModels
{

    public class LessonsViewModel : BaseViewModel
    {

        int _lessonGroupId;

        Lesson _selectedItem;
        public ObservableCollection<Lesson> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command<Lesson> ItemTapped { get; }

        public LessonsViewModel(int lessonGroupId)
        {
            _lessonGroupId = lessonGroupId;
            Items = new ObservableCollection<Lesson>();
            LoadItemsCommand = new Command(async () => await DisplayLessons());
            ItemTapped = new Command<Lesson>(OnItemSelected);

        }


        async Task DisplayLessons()
        {

            IsBusy = true;
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

            IsBusy = false;
        }


        public async void OnAppearing()
        {
            Title = Title = (await DependencyService.Get<ISqliteService>().GetLessonGroupAsync(_lessonGroupId)).Name;
            IsBusy = true;
            SelectedItem = null;
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