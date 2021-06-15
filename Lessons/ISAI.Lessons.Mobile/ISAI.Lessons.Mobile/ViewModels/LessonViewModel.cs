using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.EntityFramework.ViewModels;
using ISAI.Lessons.Mobile.Models;
using ISAI.Lessons.Mobile.Models.Messages;
using ISAI.Lessons.Mobile.Services;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using MediaManager;
using MediaManager.Library;
using MediaManager.Player;
using Plugin.DeviceOrientation;
using Plugin.DeviceOrientation.Abstractions;
using Plugin.Hud;
using Plugin.Hud.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.ViewModels
{

    public class LessonViewModel : BaseViewModel
    {
        int _lessonId;

   
        public Command WatchCommand { get; }
        public Command DownloadCommand { get; }
        public Command DeleteDownload { get; }



        public VideoDownload VideoDownload
        {
            get => _videoDownload;
            set
            {
                SetProperty(ref _videoDownload, value);
            }
        }
        VideoDownload _videoDownload;

        public bool CanDownload
        {
            get => _canDownload;
            set
            {
                SetProperty(ref _canDownload, value);
            }
        }
        bool _canDownload;


        public bool CanDeleteDownload
        {
            get => _canDeleteDownload;
            set
            {
                SetProperty(ref _canDeleteDownload, value);
            }
        }
        bool _canDeleteDownload;


        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                SetProperty(ref _isDownloading, value);
            }
        }
        bool _isDownloading;


        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                SetProperty(ref _imageUrl, value);
            }
        }
        string _imageUrl;


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

        public string DownloadErrorText
        {
            get => _downloadErrorText;
            set
            {
                SetProperty(ref _downloadErrorText, value);

            }
        }
        string _downloadErrorText = string.Empty;


        public string SourceUrl
        {
            get => _sourceUrl;
            set
            {
                SetProperty(ref _sourceUrl, value);

            }
        }
        string _sourceUrl = "https://scottishonlinelessons.com/";


        public LessonViewModel(int lessonId)
        {
            _lessonId = lessonId;
            CanDownload = true;
            CanDeleteDownload = false;


            ImageUrl = string.Format("https://scottishonlinelessons.com/assets/lessonthumbs/thumb_{0}.jpg", _lessonId);
            WatchCommand = new Command(OnWatchClicked);
            DownloadCommand = new Command(OnDownloadClicked);
            DeleteDownload = new Command(OnDeleteDownloadClicked);

            MessagingCenter.Subscribe<DownloadCompleteMessage>(this, "DownloadComplete", (sender) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await CheckDownloadStatus();
                });
            });

            //MessagingCenter.Subscribe<DownloadDidUpdateMessage>(this, "DownloadUpdate", (sender) =>
            //{
            //    MainThread.BeginInvokeOnMainThread(async () =>
            //    {
            //        await CheckDownloadStatus();
            //    });

            //});


        }

        public async void OnAppearing()
        {
            var db = DependencyService.Get<ISqliteService>();

            Lesson = await db.GetLessonAsync(_lessonId);

            await CheckDownloadStatus();

            Title = Lesson.Name;

            var lessongroupHierarchy = await db.GetLessonGroupHierarchyAsync(Lesson.LessonGroupId);
            Breadcrumb = string.Join(" > ", lessongroupHierarchy.Select(x => x.Name));

        }

        async Task CheckDownloadStatus() {

            VideoDownload = await DependencyService.Get<ISqliteService>().GetVideoDownloadForLessonAsync(_lessonId);
            DownloadErrorText = string.Empty;

            CanDeleteDownload = false;
            CanDownload = false;
            IsDownloading = false;

            if (VideoDownload != null)
            {
                if (VideoDownload.VideoDownloadStatusCode == VideoDownloadStatusCode.Successful)
                {
                    CanDeleteDownload = true;
 
                }
                else
                {
                    IsDownloading = true;
                }

            }
            else
            {
                var videoDownloadCount = (await DependencyService.Get<ISqliteService>().GetVideoDownloadsAsync()).Count;

                if (videoDownloadCount >= 3)
                {
                    DownloadErrorText = "Maximum videos downloaded. To download this video please delete one of the videos already downloaded.";
                }
                else
                {
                    CanDownload = true;
                  
                }

            }
    
        }

        async void OnWatchClicked(object obj)
        {

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {

                if (VideoDownload != null)
                {

                    var videoPath = DependencyService.Get<IVideoDownloadService>().GetLocalVideoPath(_videoDownload);

                    var lessonPage = new VideoPageAndroid(_lesson.Id, videoPath);
                    await Shell.Current.Navigation.PushModalAsync(lessonPage, true);
                    CrossHud.Current.Dismiss();
                }
                else
                {

                    CrossHud.Current.Show("Preparing video", -1, MaskType.Black);

                    var apiService = new ApiService(false, DependencyService.Get<IAuthService>());

                    var lessonRequestViewModel = new LessonRequestViewModel()
                    {
                        LessonId = _lesson.Id,
                        RemoteMediaType = RemoteMediaType.StandardStream,
                        CustomerDevice = new CustomerDeviceViewModel()
                        {
                            DeviceIdentifier = AppIdService.GetAppId(),
                            DeviceType = DeviceInfo.Platform.ToString(),
                            Name = string.Format("{0} ({1} {2})", DeviceInfo.Name, DeviceInfo.Manufacturer, DeviceInfo.Model).Trim()
                        }
                    };

                    var streamingUrlResponse = await apiService.GetLessonStreamingUrlAsync(lessonRequestViewModel);
                    CrossHud.Current.Dismiss();

                    if (streamingUrlResponse.Status == ResponseStatus.OK)
                    {
                        var lessonPage = new VideoPageAndroid(_lesson.Id, streamingUrlResponse.Content.StreamingUrl);
                        await Shell.Current.Navigation.PushModalAsync(lessonPage, true);
                    }
                    else if(streamingUrlResponse.ErrorResponse != null)
                    {
                        CrossHud.Current.ShowError(streamingUrlResponse.ErrorResponse[0].Message, MaskType.Black, TimeSpan.FromSeconds(5));
                    }
                    else
                    {
                        
                        CrossHud.Current.ShowError("Cannot stream at this time.", MaskType.Black, TimeSpan.FromSeconds(5));
                    }

                   

                }                  

            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {

                if (VideoDownload != null)
                {

                    var videoPath = DependencyService.Get<IVideoDownloadService>().GetLocalVideoPath(_videoDownload);
                    var fileName = Path.GetFileName(videoPath);

                    var lessonPage = new VideoPage(_lesson.Id, "ms-appdata:///local/" + fileName);
                    await Shell.Current.Navigation.PushAsync(lessonPage, true);
                    CrossHud.Current.Dismiss();
                }
                else
                {
                    CrossHud.Current.Show("Preparing video", -1, MaskType.Black);

                    var apiService = new ApiService(false, DependencyService.Get<IAuthService>());

                    var lessonRequestViewModel = new LessonRequestViewModel()
                    {
                        LessonId = _lesson.Id,
                        RemoteMediaType = RemoteMediaType.StandardStream,
                        CustomerDevice = new CustomerDeviceViewModel()
                        {
                            DeviceIdentifier = AppIdService.GetAppId(),
                            DeviceType = DeviceInfo.Platform.ToString(),
                            Name = string.Format("{0} ({1} {2})", DeviceInfo.Name, DeviceInfo.Manufacturer, DeviceInfo.Model).Trim()
                        }
                    };

                    var streamingUrlResponse = await apiService.GetLessonStreamingUrlAsync(lessonRequestViewModel);

                    if (streamingUrlResponse.Status == ResponseStatus.OK)
                    {
                        var lessonPage = new VideoPage(_lesson.Id, streamingUrlResponse.Content.StreamingUrl);
                        await Shell.Current.Navigation.PushAsync(lessonPage, true);
                    }
                    else if (streamingUrlResponse.ErrorResponse != null)
                    {
                        CrossHud.Current.ShowError(streamingUrlResponse.ErrorResponse[0].Message, MaskType.Black, TimeSpan.FromSeconds(5));
                    }
                    else
                    {

                        CrossHud.Current.ShowError("Cannot stream at this time.", MaskType.Black, TimeSpan.FromSeconds(5));
                    }


                    CrossHud.Current.Dismiss();

                }

            }

        }

        public void OnDisappearing()
        {
            CrossHud.Current.Dismiss();

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

                var lessonRequestViewModel = new LessonRequestViewModel()
                {
                    LessonId = _lesson.Id,
                    RemoteMediaType = RemoteMediaType.Download,
                    CustomerDevice = new CustomerDeviceViewModel()
                    {
                        DeviceIdentifier = AppIdService.GetAppId(),
                        DeviceType = DeviceInfo.Platform.ToString(),
                        Name = string.Format("{0} ({1} {2})", DeviceInfo.Name, DeviceInfo.Manufacturer, DeviceInfo.Model).Trim()
                    }
                };

                var streamingUrlResponse = await apiService.GetLessonStreamingUrlAsync(lessonRequestViewModel);

                if (streamingUrlResponse.Status == ResponseStatus.OK)
                {

                    VideoDownload = new VideoDownload()
                    {
                        Id = Guid.NewGuid(),
                        LessonId = _lesson.Id,
                        LessonName = _lesson.Name,
                        LessonGroup = lessonGroup.Name,
                        DownloadUrl = streamingUrlResponse.Content.StreamingUrl,
                        VideoDownloadStatusCode = VideoDownloadStatusCode.Running
                    };

                    VideoDownload = DependencyService.Get<IVideoDownloadService>().StartDownload(VideoDownload);

                    await db.SaveVideoDownloadAsync(VideoDownload);
                    await CheckDownloadStatus();
                    CrossHud.Current.Dismiss();


                }
                else if (streamingUrlResponse.ErrorResponse != null)
                {
                    CrossHud.Current.ShowError(streamingUrlResponse.ErrorResponse[0].Message, MaskType.Black, TimeSpan.FromSeconds(5));
                }
                else
                {

                    CrossHud.Current.ShowError("Cannot download at this time.", MaskType.Black, TimeSpan.FromSeconds(5));
                }

            } else
            {

                CrossHud.Current.ShowError("You must allow access to download files", MaskType.Black);

            }

        }

        async void OnDeleteDownloadClicked(object obj)
        {

            var result = await Application.Current.MainPage.DisplayAlert("Delete Download", "Are you sure you want to delete this download?", "Yes", "Cancel");

            if(result)
            {
                DependencyService.Get<IVideoDownloadService>().DeleteDownload(_videoDownload);
                await DependencyService.Get<ISqliteService>().DeleteVideoDownloadAsync(_videoDownload);
                await CheckDownloadStatus();
            }

           


        }




    }
}