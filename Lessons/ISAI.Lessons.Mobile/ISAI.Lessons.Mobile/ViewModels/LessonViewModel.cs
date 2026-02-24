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
using Newtonsoft.Json;
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
        string _sourceUrl = "https://portal.scottishonlinelessons.com";


        public LessonViewModel(int lessonId)
        {
            _lessonId = lessonId;
            _downloadedFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, _lessonId.ToString());
            ImageUrl = string.Format("{0}/VideoHandler.ashx?lessonThumbId={1}", _sourceUrl, _lessonId);

            CanDownload = true;
            CanDeleteDownload = false;

            WatchCommand = new Command(OnWatchClicked);
            DownloadCommand = new Command(OnDownloadClicked);
            DeleteDownload = new Command(OnDeleteDownloadClicked);

        }

        public void OnAppearing()
        {
            var db = DependencyService.Get<ISqliteService>();

            Lesson = db.GetLesson(_lessonId);
            Title = Lesson.Name;

            var lessongroupHierarchy = db.GetLessonGroupHierarchy(Lesson.LessonGroupId);
            Breadcrumb = string.Join(" > ", lessongroupHierarchy.Select(x => x.Name));

            CheckDownloadStatus();


        }

        void CheckDownloadStatus() {

            videoDownload = DependencyService.Get<ISqliteService>().GetVideoDownloadForLesson(_lessonId);
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

                var videoDownloadCount = (DependencyService.Get<ISqliteService>().GetVideoDownloads()).Count;

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
          
            VideoVersion? selectedVersion = VideoVersion.Standard;

            if (Lesson.IsSignedAvailable)
            {
                var versionChoice = await Shell.Current.CurrentPage.DisplayActionSheet(
                    "Choose version", 
                    "Cancel", 
                    null, 
                    "Standard video", 
                    "British Sign Language video");

                if (versionChoice == "Cancel")
                {
                    return;
                }

                selectedVersion = versionChoice == "British Sign Language video" 
                    ? VideoVersion.SignLanguage 
                    : VideoVersion.Standard;
            }

            if (videoDownload != null)
            {
                var lessonPage = new VideoPage(Lesson, selectedVersion == VideoVersion.Standard ? videoDownload.DownloadUrl : videoDownload.DownloadSignedUrl);
                await Shell.Current.Navigation.PushAsync(lessonPage, true);
                DependencyService.Get<IHud>().Dismiss();
            }
            else
            {
                // Streamed video playback
                DependencyService.Get<IHud>().ShowSpinner("Preparing video");

                var apiService = new ApiService(DependencyService.Get<IAuthService>(), Constants.BasePortalUrl, Constants.BaseReturnUrl);

                var lessonRequestViewModel = new LessonRequestViewModel()
                {
                    LessonId = Lesson.Id,
                    RemoteMediaType = RemoteMediaType.EncryptedStream,
                    CustomerDevice = new CustomerDeviceViewModel()
                    {
                        DeviceIdentifier = AppIdService.GetAppId(),
                        DeviceType = DeviceInfo.Platform.ToString(),
                        Name = string.Format("{0} ({1} {2})", DeviceInfo.Name, DeviceInfo.Manufacturer, DeviceInfo.Model).Trim()
                    }
                };

                Console.Write("ISAI:" + JsonConvert.SerializeObject(lessonRequestViewModel));

                Console.Write("ISAI:" + lessonRequestViewModel.LessonId + lessonRequestViewModel.RemoteMediaType.ToString() + lessonRequestViewModel.CustomerDevice.Name + lessonRequestViewModel.CustomerDevice.DeviceIdentifier + lessonRequestViewModel.CustomerDevice.DeviceType);

                var streamingUrlResponse = await apiService.GetLessonStreamingUrlAsync(lessonRequestViewModel);
                DependencyService.Get<IHud>().Dismiss();

                if (streamingUrlResponse.Status == ResponseStatus.OK)
                {
                    // Select appropriate streaming URL based on version choice
                    string selectedStreamingUrl = streamingUrlResponse.Content?.StreamingUrl ?? string.Empty;

                    if (selectedVersion == VideoVersion.SignLanguage && !string.IsNullOrWhiteSpace(streamingUrlResponse.Content?.StreamingSignedUrl))
                    {
                        selectedStreamingUrl = streamingUrlResponse.Content.StreamingSignedUrl;
                    }

                    var lessonPage = new VideoPage(Lesson, _sourceUrl + selectedStreamingUrl);
                    await Shell.Current.Navigation.PushAsync(lessonPage, true);
                }
                else if (streamingUrlResponse.ErrorResponse != null)
                {
                    DependencyService.Get<IHud>().ShowError(streamingUrlResponse.ErrorResponse[0].Message, TimeSpan.FromSeconds(3));
                }
                else
                {
                    DependencyService.Get<IHud>().ShowError("Cannot stream at this time.", TimeSpan.FromSeconds(3));
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
                var lessonGroup = db.GetLessonGroup(Lesson.LessonGroupId);
                var apiService = new ApiService(DependencyService.Get<IAuthService>(), Constants.BasePortalUrl, Constants.BaseReturnUrl);

                var lessonRequestViewModel = new LessonRequestViewModel()
                {
                    LessonId = Lesson.Id,
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

                    var downloadStatus = await DownloadFileAsync(streamingUrlResponse.Content?.StreamingUrl, _downloadedFilePath, Lesson.Id + ".zip");

                    if (downloadStatus)
                    {

                        ZipService.ExtractZipFile(_downloadedFilePath + "/" + Lesson.Id + ".zip", _downloadedFilePath);

                        var lessonFilePath = Path.Combine(_downloadedFilePath, "m3u8");
                        var lessonSignedFilePath = Path.Combine(_downloadedFilePath, "m3u8-signed");

                        if (Directory.Exists(lessonFilePath))
                        {
                            //Legacy support for older downloads where the m3u8 file was in the root of the zip rather than in an m3u8 folder
                            var m3u8FilePath = lessonFilePath + "/" + Lesson.Id + ".m3u8";
                            var searchText = "#EXT-X-KEY:METHOD=AES-128,URI=\"";

                            var m3u8FileText = File.ReadAllText(m3u8FilePath);
                            var keyStartLocation = m3u8FileText.IndexOf(searchText) + searchText.Length;
                            var keyEndLocation = m3u8FileText.IndexOf("\"", keyStartLocation);
                            var currentKeyLocationText = m3u8FileText.Substring(keyStartLocation, keyEndLocation - keyStartLocation);
                            var deviceKeyLocationText = string.Format("{0}.m3u8.key", Lesson.Id);

                            m3u8FileText = m3u8FileText.Replace(currentKeyLocationText, deviceKeyLocationText);

                            File.WriteAllText(m3u8FilePath, m3u8FileText);

                            // Rewrite key location for signed version if available
                            if (Directory.Exists(lessonSignedFilePath))
                            {
                                var m3u8SignedFilePath = lessonSignedFilePath + "/" + Lesson.Id + ".m3u8";

                                if (File.Exists(m3u8SignedFilePath))
                                {
                                    searchText = "#EXT-X-KEY:METHOD=AES-128,URI=\"";

                                    m3u8FileText = File.ReadAllText(m3u8SignedFilePath);
                                    keyStartLocation = m3u8FileText.IndexOf(searchText) + searchText.Length;
                                    keyEndLocation = m3u8FileText.IndexOf("\"", keyStartLocation);
                                    currentKeyLocationText = m3u8FileText.Substring(keyStartLocation, keyEndLocation - keyStartLocation);
                                    deviceKeyLocationText = string.Format("{0}.m3u8.key", Lesson.Id);

                                    m3u8FileText = m3u8FileText.Replace(currentKeyLocationText, deviceKeyLocationText);

                                    File.WriteAllText(m3u8SignedFilePath, m3u8FileText);
                                }
                            }


                            videoDownload = new VideoDownload()
                            {
                                Id = Guid.NewGuid(),
                                LessonId = Lesson.Id,
                                LessonName = Lesson.Name,
                                LessonGroup = Breadcrumb,
                                DateDownloaded = DateTime.Now,
                                DownloadUrl = string.Format("http://localhost:9696/{0}/m3u8/{1}.m3u8", Lesson.Id, Lesson.Id),
                                DownloadSignedUrl = string.Format("http://localhost:9696/{0}/m3u8-signed/{1}.m3u8", Lesson.Id, Lesson.Id),

                            };

                            DependencyService.Get<ISqliteService>().SaveVideoDownload(videoDownload);

                        }
                        else
                        {
                            //Legacy support for older downloads where the m3u8 file was in the root of the zip rather than in an m3u8 folder
                            var m3u8FilePath = _downloadedFilePath + "/" + Lesson.Id + ".m3u8";
                            var searchText = "#EXT-X-KEY:METHOD=AES-128,URI=\"";

                            var m3u8FileText = File.ReadAllText(m3u8FilePath);
                            var keyStartLocation = m3u8FileText.IndexOf(searchText) + searchText.Length;
                            var keyEndLocation = m3u8FileText.IndexOf("\"", keyStartLocation);
                            var currentKeyLocationText = m3u8FileText.Substring(keyStartLocation, keyEndLocation - keyStartLocation);
                            var deviceKeyLocationText = string.Format("{0}.m3u8.key", Lesson.Id);

                            m3u8FileText = m3u8FileText.Replace(currentKeyLocationText, deviceKeyLocationText);

                            File.WriteAllText(m3u8FilePath, m3u8FileText);

                            videoDownload = new VideoDownload()
                            {
                                Id = Guid.NewGuid(),
                                LessonId = Lesson.Id,
                                LessonName = Lesson.Name,
                                LessonGroup = Breadcrumb,
                                DateDownloaded = DateTime.Now,
                                DownloadUrl = string.Format("http://localhost:9696/{0}/{1}.m3u8", Lesson.Id, Lesson.Id),
                                DownloadSignedUrl = null

                            };

                            DependencyService.Get<ISqliteService>().SaveVideoDownload(videoDownload);

                        }

                        // Debug: List all files in AppDataDirectory
                        LogAppDataDirectoryContents();

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
                client.BaseAddress = new Uri(Constants.BaseReturnUrl);

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
            var result = await Shell.Current.CurrentPage.DisplayAlert("Delete Download", "Are you sure you want to delete this download?", "Yes", "Cancel");

            if(result)
            {
                Directory.Delete(_downloadedFilePath, true);
                DependencyService.Get<ISqliteService>().DeleteVideoDownload(videoDownload);
                CheckDownloadStatus();
            }
        }

        private void LogAppDataDirectoryContents()
        {
            try
            {
                var appDataDir = FileSystem.Current.AppDataDirectory;
                Console.WriteLine($"=== Listing all files in AppDataDirectory: {appDataDir} ===");
                
                LogDirectoryRecursive(appDataDir, 0);
                
                var allFiles = Directory.GetFiles(appDataDir, "*.*", SearchOption.AllDirectories);
                var allDirs = Directory.GetDirectories(appDataDir, "*", SearchOption.AllDirectories);
                
                Console.WriteLine($"=== Total Files: {allFiles.Length}, Total Directories: {allDirs.Length} ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing files: {ex.Message}");
            }
        }

        private void LogDirectoryRecursive(string path, int level)
        {
            try
            {
                var indent = new string(' ', level * 2);
                
                // Log directories
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    var dirName = Path.GetFileName(dir);
                    Console.WriteLine($"{indent}[DIR]  {dirName}/");
                    LogDirectoryRecursive(dir, level + 1);
                }
                
                // Log files
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var fileInfo = new FileInfo(file);
                    Console.WriteLine($"{indent}[FILE] {fileName} ({fileInfo.Length:N0} bytes)");
                }
            }
            catch (Exception ex)
            {
                var indent = new string(' ', level * 2);
                Console.WriteLine($"{indent}Error accessing {path}: {ex.Message}");
            }
        }

    }
}