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
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.ViewModels
{
    public class LessonDownloadsViewModel : BaseViewModel
    {

        VideoDownload _selectedItem;
        public ObservableCollection<VideoDownload> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command DeleteAllCommand { get; }
        public Command<VideoDownload> ItemTapped { get; }

        public LessonDownloadsViewModel()
        {
            Items = new ObservableCollection<VideoDownload>();
            LoadItemsCommand = new Command(async () => await DisplayVideoDownloads());
            ItemTapped = new Command<VideoDownload>(OnItemSelected);
            DeleteAllCommand = new Command(async () => await DeleteAllVideoDownloads());
            Title = "Lesson Downloads";
        }


        async Task DisplayVideoDownloads()
        {

            IsBusy = true;

            Items.Clear();

            var lessons = (await DependencyService.Get<ISqliteService>().GetVideoDownloadsAsync());

            foreach (var item in lessons)
            {
                Items.Add(item);
            }

            IsBusy = false;
        }


        public async void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public VideoDownload SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async Task DeleteAllVideoDownloads()
        {
            DependencyService.Get<IVideoDownloadService>().DeleteAllDownloads();
            await DependencyService.Get<ISqliteService>().DeleteAllVideoDownloadsAsync();
            await DisplayVideoDownloads();
        }

        async void OnItemSelected(VideoDownload item)
        {
            if (item == null)
                return;

            _selectedItem = item;

            var lessonPage = new LessonPage(_selectedItem.LessonId);
            await Shell.Current.Navigation.PushAsync(lessonPage, true);

        }
    }
}