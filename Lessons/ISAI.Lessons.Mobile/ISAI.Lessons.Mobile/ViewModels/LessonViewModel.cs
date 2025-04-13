using ISAI.Lessons.Core.Services;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.EntityFramework.ViewModels;
using ISAI.Lessons.Mobile.Services;
using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Models.App;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISAI.Lessons.Mobile.ViewModels
{

    public class LessonViewModel : BaseViewModel
    {
        private int _lessonId;

        private string _downloadedFilePath;

        private VideoDownload videoDownload;


        public Command WatchCommand { get; }
        public Command DownloadCommand { get; }
        public Command DeleteDownload { get; }

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
        string _sourceUrl = "https://portal.scottishonlinelessons.com/";


        public LessonViewModel(int lessonId)
        {
            _lessonId = lessonId;
            _downloadedFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, _lessonId.ToString());
            ImageUrl = string.Format("{0}VideoHandler.ashx?lessonThumbId={1}", _sourceUrl, _lessonId);

            CanDownload = true;
            CanDeleteDownload = false;

            WatchCommand = new Command(OnWatchClicked);
            DownloadCommand = new Command(OnDownloadClicked);
            DeleteDownload = new Command(OnDeleteDownloadClicked);

        }

        public async void OnAppearing()
        {
            var db = DependencyService.Get<ISqliteService>();

            Lesson = await db.GetLessonAsync(_lessonId);
            Title = Lesson.Name;

            var lessongroupHierarchy = await db.GetLessonGroupHierarchyAsync(Lesson.LessonGroupId);
            Breadcrumb = string.Join(" > ", lessongroupHierarchy.Select(x => x.Name));

            await CheckDownloadStatus();


        }

        async Task CheckDownloadStatus() {

            videoDownload = await DependencyService.Get<ISqliteService>().GetVideoDownloadForLessonAsync(_lessonId);
            DownloadErrorText = string.Empty;

            if (videoDownload != null)
            {
                CanDeleteDownload = true;
                CanDownload = false;
            }
            else
            {
                CanDeleteDownload = false;
                CanDownload = false;

                var videoDownloadCount = (await DependencyService.Get<ISqliteService>().GetVideoDownloadsAsync()).Count;

                if (videoDownloadCount >= 10)
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
            if (videoDownload != null)
            {
                var lessonPage = new VideoPage(_lesson.Id, videoDownload.DownloadUrl);
  
                await Shell.Current.Navigation.PushAsync(lessonPage, true);
                DependencyService.Get<IHud>().Dismiss();
            }
            else
            {


                DependencyService.Get<IHud>().ShowSpinner("Preparing video");

                var apiService = new ApiService(DependencyService.Get<IAuthService>(), Constants.BasePortalUrl, Constants.BaseReturnUrl);

                var lessonRequestViewModel = new LessonRequestViewModel()
                {
                    LessonId = _lesson.Id,
                    RemoteMediaType = RemoteMediaType.EncryptedStream,
                    CustomerDevice = new CustomerDeviceViewModel()
                    {
                        DeviceIdentifier = AppIdService.GetAppId(),
                        DeviceType = DeviceInfo.Platform.ToString(),
                        Name = string.Format("{0} ({1} {2})", DeviceInfo.Name, DeviceInfo.Manufacturer, DeviceInfo.Model).Trim()
                    }
                };

                var streamingUrlResponse = await apiService.GetLessonStreamingUrlAsync(lessonRequestViewModel);
                DependencyService.Get<IHud>().Dismiss();

                if (streamingUrlResponse.Status == ResponseStatus.OK)
                {
                    var lessonPage = new VideoPage(_lesson.Id, _sourceUrl + "/" + streamingUrlResponse.Content.StreamingUrl);
                    await Shell.Current.Navigation.PushAsync(lessonPage, true);
                }
                else if (streamingUrlResponse.ErrorResponse != null)
                {
                    DependencyService.Get<IHud>().ShowError(streamingUrlResponse.ErrorResponse[0].Message, TimeSpan.FromSeconds(5));
              }
                else
                {
                    DependencyService.Get<IHud>().ShowError("Cannot stream at this time.", TimeSpan.FromSeconds(5));
                }


            }
            
        }

        public void OnDisappearing()
        {
            DependencyService.Get<IHud>().Dismiss();

        }


        async void OnDownloadClicked(object obj)
        {

            DependencyService.Get<IHud>().ShowSpinner("Preparing Download");

            var readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
            var writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

            if (readStatus == PermissionStatus.Granted && writeStatus == PermissionStatus.Granted)
            {

                var db = DependencyService.Get<ISqliteService>();
                var lessonGroup = await db.GetLessonGroupAsync(_lesson.LessonGroupId);
                var apiService = new ApiService(DependencyService.Get<IAuthService>(), Constants.BasePortalUrl, Constants.BaseReturnUrl);

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

                    DependencyService.Get<IHud>().ShowSpinner("Downloading Lesson. This may take a few moments.");

                    var downloadUrl = _sourceUrl + streamingUrlResponse.Content.StreamingUrl;
                  
                    var downloadStatus = await DownloadFileAsync(downloadUrl, _downloadedFilePath, _lesson.Id + ".zip");

                    if (downloadStatus)
                    {

                        ZipService.ExtractZipFile(_downloadedFilePath + "/" + _lesson.Id + ".zip", _downloadedFilePath);

                        //Rewrite the m3u8.key key location
                        var m3u8FilePath = _downloadedFilePath + "/" + _lesson.Id + ".m3u8";
                        var searchText = "#EXT-X-KEY:METHOD=AES-128,URI=\"";

                        var m3u8FileText = File.ReadAllText(m3u8FilePath);
                        var keyStartLocation = m3u8FileText.IndexOf(searchText) + searchText.Length;
                        var keyEndLocation = m3u8FileText.IndexOf("\"", keyStartLocation);
                        var currentKeyLocationText = m3u8FileText.Substring(keyStartLocation, keyEndLocation - keyStartLocation);
                        var deviceKeyLocationText = string.Format("{0}.m3u8.key", _lesson.Id);

                        m3u8FileText = m3u8FileText.Replace(currentKeyLocationText, deviceKeyLocationText);

                        File.WriteAllText(m3u8FilePath, m3u8FileText);

                        videoDownload = new VideoDownload()
                        {
                            Id = Guid.NewGuid(),
                            LessonId = _lesson.Id,
                            LessonName = Lesson.Name,
                            LessonGroup = Breadcrumb,
                            DateDownloaded = DateTime.Now,
                            DownloadUrl = m3u8FilePath,

                        };

                        await DependencyService.Get<ISqliteService>().SaveVideoDownloadAsync(videoDownload);

                        CanDeleteDownload = true;
                        CanDownload = false;

                        DependencyService.Get<IHud>().Dismiss();

                    } else
                    {
                        DependencyService.Get<IHud>().ShowError("Cannot download at this time.", TimeSpan.FromSeconds(3));
                    }


                }
                else if (streamingUrlResponse.ErrorResponse != null)
                {
                    DependencyService.Get<IHud>().ShowError(streamingUrlResponse.ErrorResponse[0].Message, TimeSpan.FromSeconds(3));
                }
                else
                {
                    DependencyService.Get<IHud>().ShowError("Cannot download at this time.", TimeSpan.FromSeconds(3));
                }

            } else
            {

                DependencyService.Get<IHud>().ShowError("You must allow access to download files", TimeSpan.FromSeconds(3));
             
            }

        }

        private async Task<bool> DownloadFileAsync(string fileUrl, string downloadedFilePath, string downloadFileName)
        {
            try
            {
                using var client = new HttpClient();

                var downloadStream = await client.GetByteArrayAsync(fileUrl);

                if (!Directory.Exists(downloadedFilePath))
                    Directory.CreateDirectory(downloadedFilePath);

                var filePath = downloadedFilePath + "/" + downloadFileName;

                if (File.Exists(filePath))
                    File.Delete(filePath);

                File.WriteAllBytes(downloadedFilePath + "/" + downloadFileName, downloadStream);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        async void OnDeleteDownloadClicked(object obj)
        {

            var result = await Application.Current.MainPage.DisplayAlert("Delete Download", "Are you sure you want to delete this download?", "Yes", "Cancel");

            if(result)
            {
                Directory.Delete(_downloadedFilePath, true);
                await DependencyService.Get<ISqliteService>().DeleteVideoDownloadAsync(videoDownload);
                await CheckDownloadStatus();
            }

          
        }




    }
}