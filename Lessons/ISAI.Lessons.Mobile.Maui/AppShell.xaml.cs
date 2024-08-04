using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.Mobile.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await DependencyService.Get<ISqliteService>().DeleteDatabaseAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
