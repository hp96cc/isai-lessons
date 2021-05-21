using ISAI.Lessons.Mobile.Models;
using ISAI.Lessons.Mobile.Models.Messages;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using MediaManager;
using MediaManager.Library;
using MediaManager.Player;
using Plugin.Hud;
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

    public class VideoViewModel : BaseViewModel
    {
        int _lessonId;
        Lesson _lesson;
        string _streamingUrl;


        public string SourceUrl
        {
            get => _sourceUrl;
            set
            {
                SetProperty(ref _sourceUrl, value);

            }
        }
        string _sourceUrl = "https://scottishonlinelessons.com/";


        public VideoViewModel(int lessonId, string streamingUrl)
        {
            _lessonId = lessonId;
            _streamingUrl = streamingUrl;
            SourceUrl = streamingUrl;

      
        }

        public async void OnAppearing()
        {
  

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {

                _lesson = await DependencyService.Get<ISqliteService>().GetLessonAsync(_lessonId);
                Title = _lesson.Name;

                DependencyService.Get<IStatusBar>().HideStatusBar();

                CrossHud.Current.Show();
                CrossMediaManager.Current.Init();
                CrossMediaManager.Current.Notification.Enabled = false;

                CrossMediaManager.Current.StateChanged += Current_StateChanged;

                var videoDownload = await DependencyService.Get<ISqliteService>().GetVideoDownloadForLessonAsync(_lessonId);
                IMediaItem item;

                if (videoDownload != null)
                {
                    var videoPath = DependencyService.Get<IVideoDownloadService>().GetLocalVideoPath(videoDownload);
                    item = await CrossMediaManager.Current.Extractor.CreateMediaItem(videoPath);
                }
                else
                {
                    item = await CrossMediaManager.Current.Extractor.CreateMediaItem(_streamingUrl);
                    item.MediaType = MediaType.Hls;
                }


                item.Title = _lesson.Name;

                await CrossMediaManager.Current.Play(item);

            }

        }

        public async void OnDisappearing()
        {

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                CrossHud.Current.Dismiss();
                DependencyService.Get<IStatusBar>().HideStatusBar();

                CrossMediaManager.Current.StateChanged -= Current_StateChanged;
                await CrossMediaManager.Current.Stop();
                CrossMediaManager.Current.Dispose();

            }
        }

        private void Current_StateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
        {
            if (e.State == MediaPlayerState.Buffering)
            {
                CrossHud.Current.Show();
            }
            else
            {
                CrossHud.Current.Dismiss();
            }
        }


    }
}