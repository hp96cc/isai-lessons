using ISAI.Lessons.Mobile.Views;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace ISAI.Lessons.Mobile.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {

        public ICommand BrowseLessonsCommand { get; }

        public ICommand  DownloadedLessonsCommaned { get; }

        public AboutViewModel()
        {
            Title = "Scottish Online Lessons";

            BrowseLessonsCommand = new Command(async () => await BrowseLessons());
            DownloadedLessonsCommaned = new Command(async () => await DownloadedLessons());
        }

        async Task DownloadedLessons()
        {
            await Shell.Current.GoToAsync($"//{nameof(LessonDownloadsPage)}");
        }

        async Task BrowseLessons()
        {
            await Shell.Current.GoToAsync($"//{nameof(LessonGroupPage)}");
        }
    }
}