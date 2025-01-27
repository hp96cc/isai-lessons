using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using ISAI.Lessons.Models.Interfaces;
using Font = Microsoft.Maui.Font;

namespace ISAI.Lessons.Mobile;

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