using ISAI.Lessons.Mobile.Models;
using ISAI.Lessons.Mobile.Models.Messages;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ISAI.Lessons.Mobile.ViewModels
{
    public class LessonDownloadsViewModel : BaseViewModel
    {

        bool _hasAppeared;

        VideoDownload _selectedItem;
        public ObservableCollection<VideoDownload> Items { get; }
        public Command DeleteAllCommand { get; }
        public Command<VideoDownload> ItemTapped { get; }

        public LessonDownloadsViewModel()
        {
            Items = new ObservableCollection<VideoDownload>();
            ItemTapped = new Command<VideoDownload>(OnItemSelected);
            DeleteAllCommand = new Command(async () => await DeleteAllVideoDownloads());
            Title = "Lesson Downloads";

            MessagingCenter.Subscribe<DownloadCompleteMessage>(this, "DownloadComplete", (sender) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayVideoDownloads(true);
                });
              
            });

        }


        async Task DisplayVideoDownloads(bool forceReload = false)
        {

            if (_hasAppeared && !forceReload) return;
            _hasAppeared = true;

            Items.Clear();

            var lessons = (await DependencyService.Get<ISqliteService>().GetVideoDownloadsAsync());

            foreach (var item in lessons)
            {
                Items.Add(item);
            }

     
        }


        public async void OnAppearing()
        {
            SelectedItem = null;
            await DisplayVideoDownloads(false);
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

            var result = await Application.Current.MainPage.DisplayAlert("Delete All Downloads", "Are you sure you want to delete all downloads?", "Yes", "Cancel");

            if (result)
            {
                DependencyService.Get<IVideoDownloadService>().DeleteAllDownloads();
                await DependencyService.Get<ISqliteService>().DeleteAllVideoDownloadsAsync();
                await DisplayVideoDownloads(true);
            }


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