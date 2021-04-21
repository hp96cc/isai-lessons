using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.Mobile.Models;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using MediaManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.ViewModels
{

    public class LessonViewModel : BaseViewModel
    {
        int _lessonId;
        Lesson _lesson;

        public Command WatchCommand { get; }
        public Command DownloadCommand { get; }

        public LessonViewModel(int lessonId)
        {
            _lessonId = lessonId;


            WatchCommand = new Command(OnWatchClicked);
            DownloadCommand = new Command(OnDownloadClicked);
        }

        public async void OnAppearing()
        {
            _lesson = await DependencyService.Get<ISqliteService>().GetLessonAsync(_lessonId);
            Title = _lesson.Name;
        }

        async void OnWatchClicked(object obj)
        {

            var lessonPage = new VideoPage(_lesson.Id);
            await Shell.Current.Navigation.PushModalAsync(lessonPage, true);

        }

        async void OnDownloadClicked(object obj)
        {
            DependencyService.Get<IVideoDownload>().StartDownload("https://file-examples-com.github.io/uploads/2017/04/file_example_MP4_1920_18MG.mp4");

        }

    }
}