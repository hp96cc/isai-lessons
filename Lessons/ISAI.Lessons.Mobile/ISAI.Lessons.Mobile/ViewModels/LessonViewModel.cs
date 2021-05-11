using ISAI.Lessons.EntityFramework.Services;
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


        public Lesson Lesson
        {
            get => _lesson;
            set
            {
                SetProperty(ref _lesson, value);

            }
        }
        Lesson _lesson;

        public string Breadcrumb
        {
            get => _breadcrumb;
            set
            {
                SetProperty(ref _breadcrumb, value);

            }
        }
        string _breadcrumb = string.Empty;

        public LessonViewModel(int lessonId)
        {
            _lessonId = lessonId;
            CanDownload = true;

            WatchCommand = new Command(OnWatchClicked);
            DownloadCommand = new Command(OnDownloadClicked);

        }

        public async void OnAppearing()
        {
            var db = DependencyService.Get<ISqliteService>();

            Lesson = await db.GetLessonAsync(_lessonId);
            _videoDownload = await DependencyService.Get<ISqliteService>().GetVideoDownloadForLessonAsync(_lessonId);
            CanDownload = _videoDownload == null;
            Title = Lesson.Name;

            var lessongroupHierarchy = await db.GetLessonGroupHierarchyAsync(Lesson.LessonGroupId);
            Breadcrumb = string.Join(" > ", lessongroupHierarchy.Select(x => x.Name)); 

        }

        async void OnWatchClicked(object obj)
        {

            CrossHud.Current.Show("Preparing video", -1, MaskType.Black);

            var apiService = new ApiService(false, DependencyService.Get<IAuthService>());

            var streamingUrlResponse = await apiService.GetLessonStreamingUrlAsync(Lesson.Id);

            if (streamingUrlResponse.Status == ResponseStatus.OK)
            {
                var lessonPage = new VideoPage(_lesson.Id, streamingUrlResponse.Content.StreamingUrl);
                await Shell.Current.Navigation.PushModalAsync(lessonPage, true);
                CrossHud.Current.Dismiss();
            }
            else
            {
                CrossHud.Current.ShowError("Cannot stream at this time.", MaskType.Black);
            }

        }

        async void OnDownloadClicked(object obj)
        {

            CrossHud.Current.Show("Preparing Download", -1, MaskType.Black);

            var readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
            var writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

            if (readStatus == PermissionStatus.Granted && writeStatus == PermissionStatus.Granted)
            {

                var db = DependencyService.Get<ISqliteService>();
                var lessonGroup = await db.GetLessonGroupAsync(_lesson.LessonGroupId);
                var apiService = new ApiService(false, DependencyService.Get<IAuthService>());

                var downloadUrlResponse = await apiService.GetLessonDownloadUrlAsync(Lesson.Id);

                if (downloadUrlResponse.Status == ResponseStatus.OK)
                {

                    _videoDownload = new VideoDownload()
                    {
                        Id = Guid.NewGuid(),
                        LessonId = _lesson.Id,
                        LessonName = _lesson.Name,
                        LessonGroup = lessonGroup.Name,
                        DownloadUrl = downloadUrlResponse.Content.DownloadUrl,
                    };

                    _videoDownload = DependencyService.Get<IVideoDownloadService>().StartDownload(_videoDownload);

                    await db.SaveVideoDownloadAsync(_videoDownload);
                    CanDownload = false;
                    CrossHud.Current.Dismiss();


                } else
                {
                    CrossHud.Current.ShowError("Cannot perform download at this time.", MaskType.Black);
                }

            } else
            {

                CrossHud.Current.ShowError("You must allow access to download files", MaskType.Black);

            }

        }

    }
}