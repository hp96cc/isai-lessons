using ISAI.Lessons.Mobile.Models;
using ISAI.Lessons.Mobile.Models.Messages;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using MediaManager;
using MediaManager.Library;
using MediaManager.Player;
//TODO IMPORT: using Plugin.Hud;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

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
        string _sourceUrl = "https://portal.scottishonlinelessons.com/";


        public VideoViewModel(int lessonId, string streamingUrl)
        {
            _lessonId = lessonId;
            _streamingUrl = streamingUrl;
            SourceUrl = streamingUrl;

        }

        private void Current_StateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
        {
            if (e.State == MediaPlayerState.Buffering)
            {
                DependencyService.Get<IHud>().ShowSpinner("Loading...");
            }
            else
            {
                DependencyService.Get<IHud>().Dismiss();
            }
        }


    }
}