using ISAI.Lessons.Models.Interfaces.App;
using Microsoft.Maui.Controls;
using System;

namespace ISAI.Lessons.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            DependencyService.Get<ISqliteService>().DeleteDatabase();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
