using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.Mobile.Models;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
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
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.ViewModels
{

    public class VideoViewModel : BaseViewModel
    {
        int _lessonId;
        Lesson _lesson;


        public VideoViewModel(int lessonId)
        {
            _lessonId = lessonId;
        }

        public async void OnAppearing()
        {
            _lesson = await DependencyService.Get<ISqliteService>().GetLessonAsync(_lessonId);
            Title = _lesson.Name;
            
            DependencyService.Get<IStatusBar>().HideStatusBar();

            CrossHud.Current.Show();
            CrossMediaManager.Current.Init();

            CrossMediaManager.Current.StateChanged += Current_StateChanged;
            var item = await CrossMediaManager.Current.Extractor.CreateMediaItem("https://lessonsmedia-ukso1.streaming.media.azure.net/52e672d1-95c9-479b-9fa7-c4d8ad152746/Using Commas.ism/manifest(format=m3u8-aapl)");
            item.MediaType = MediaType.Hls;
            item.Title = _lesson.Name;

            await CrossMediaManager.Current.Play(item);

        }

        public async void OnDisappearing()
        {
            CrossHud.Current.Dismiss();
            DependencyService.Get<IStatusBar>().HideStatusBar();
            CrossMediaManager.Current.StateChanged -= Current_StateChanged;
            await CrossMediaManager.Current.Stop();
            CrossMediaManager.Current.Dispose();
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