using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.Mobile.Services;
using ISAI.Lessons.Mobile.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ISAI.Lessons.Mobile
{
    public partial class App : Application
    {

        public static List<Lesson> Lessons { get; set; }
        public static List<LessonGroup> LessonsGroups { get; set; }
        public static bool IsLoggedIn { get; set; }

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
