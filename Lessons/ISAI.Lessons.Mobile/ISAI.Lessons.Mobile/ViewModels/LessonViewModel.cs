using ISAI.Lessons.Mobile.Models;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using MediaManager;
using Plugin.Hud;
using Plugin.Hud.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.ViewModels
{

    public class LessonViewModel : BaseViewModel
    {
        int _lessonId;
        Lesson _lesson;
        VideoDownload _videoDownload;

        public Command WatchCommand { get; }
        public Command DownloadCommand { get; }

        public bool CanDownload
        {
            get => _videoDownload == null;
            set
            {
                SetProperty(ref _canDownload, value);
            }
        }
        bool _canDownload;

        public LessonViewModel(int lessonId)
        {
            _lessonId = lessonId;
            CanDownload = true;

            WatchCommand = new Command(OnWatchClicked);
            DownloadCommand = new Command(OnDownloadClicked);

        }

        public async void OnAppearing()
        {
            _lesson = await DependencyService.Get<ISqliteService>().GetLessonAsync(_lessonId);
            _videoDownload = await DependencyService.Get<ISqliteService>().GetVideoDownloadForLessonAsync(_lessonId);
            CanDownload = _videoDownload == null;
            Title = _lesson.Name;
        }

        async void OnWatchClicked(object obj)
        {
            var lessonPage = new VideoPage(_lesson.Id);
            await Shell.Current.Navigation.PushModalAsync(lessonPage, true);
        }

        async void OnDownloadClicked(object obj)
        {
            var readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
            var writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

            if (readStatus == PermissionStatus.Granted && writeStatus == PermissionStatus.Granted)
            {

                var db = DependencyService.Get<ISqliteService>();
                var lessonGroup = await db.GetLessonGroupAsync(_lesson.LessonGroupId);

                _videoDownload = new VideoDownload()
                {
                    Id = Guid.NewGuid(),
                    LessonId = _lesson.Id,
                    LessonName = _lesson.Name,
                    LessonGroup = lessonGroup.Name,
                    DownloadUrl = "https://file-examples-com.github.io/uploads/2017/04/file_example_MP4_1920_18MG.mp4",
                };

                _videoDownload = DependencyService.Get<IVideoDownloadService>().StartDownload(_videoDownload);

                await db.SaveVideoDownloadAsync(_videoDownload);
                CanDownload = false;

            } else
            {

                CrossHud.Current.ShowError("You must allow access to dowbload files", MaskType.Black);

            }

        }

    }
}