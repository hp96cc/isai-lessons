using ISAI.Lessons.Mobile.Models.Messages;
using ISAI.Lessons.Mobile.Views;
using System.Collections.ObjectModel;
using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Models.App;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Maui.Storage;

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
        }


        void DisplayVideoDownloads(bool forceReload = false)
        {

            if (_hasAppeared && !forceReload) return;
            _hasAppeared = true;

            Items.Clear();

            var lessons = DependencyService.Get<ISqliteService>().GetVideoDownloads();

            foreach (var item in lessons)
            {
                Items.Add(item);
            }

     
        }


        public void OnAppearing()
        {
            SelectedItem = null;
            DisplayVideoDownloads(false);
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

                var downloads = DependencyService.Get<ISqliteService>().GetVideoDownloads();

                foreach(var  download in downloads)
                {
                    Directory.Delete(Path.Combine(FileSystem.Current.AppDataDirectory, download.LessonId.ToString()), true);
                }

                DependencyService.Get<IVideoDownloadService>().DeleteAllDownloads();
                DependencyService.Get<ISqliteService>().DeleteAllVideoDownloads();

                DisplayVideoDownloads(true);
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