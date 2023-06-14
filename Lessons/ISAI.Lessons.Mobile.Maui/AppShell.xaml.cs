using ISAI.Lessons.Mobile.Views;
using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.Mobile.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            //Routing.RegisterRoute(nameof(LessonsPage), typeof(LessonsPage));
            //Routing.RegisterRoute(nameof(LessonDownloadsPage), typeof(LessonDownloadsPage));

        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await DependencyService.Get<ISqliteService>().DeleteDatabaseAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
