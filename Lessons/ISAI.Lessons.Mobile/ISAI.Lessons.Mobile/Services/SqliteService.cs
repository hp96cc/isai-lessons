using ISAI.Lessons.Models.Interfaces.App;
using ISAI.Lessons.Models.Models.App;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ISAI.Lessons.Core.Services
{
    public class SqliteService : ISqliteService
    {
        public const string DatabaseFilename = "Lessons_v14.db3";

        public const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }

        static readonly Lazy<SQLiteConnection> lazyInitializer = new Lazy<SQLiteConnection>(() =>
        {
            return new SQLiteConnection(DatabasePath, Flags);
        });

        static SQLiteConnection Database => lazyInitializer.Value;

        static bool initialized = false;

        public SqliteService()
        {
     
        }

        public  void Initialize()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(LessonGroup).Name))
                {
                    Database.CreateTable<LessonGroup>(CreateFlags.None);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Lesson).Name))
                {
                   Database.CreateTable<Lesson>(CreateFlags.None);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(VideoDownload).Name))
                {
                   Database.CreateTable<VideoDownload>(CreateFlags.None);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(AppUser).Name))
                {
                   Database.CreateTable<AppUser>(CreateFlags.None);
                }

                initialized = true;
            }

        }


        public  void DeleteDatabase()
        {

           Database.DeleteAll<LessonGroup>();
           Database.DeleteAll<Lesson>();
           Database.DeleteAll<VideoDownload>();
           Database.DeleteAll<AppUser>();

            Console.WriteLine("Database deleted");

        }

        public AppUser GetUser()
        {
            return Database.Table<AppUser>().FirstOrDefault();
        }

        public void DeleteUser(AppUser appUser)
        {
           Database.Delete<AppUser>(appUser.Id);
        }

        public void SaveUser(AppUser appUser)
        {
           Database.InsertOrReplace(appUser);
        }

        public  List<LessonGroup> GetLessonGroups(int? parentId = null)
        {
            return Database.Table<LessonGroup>().Where(x => x.ParentLessonGroupId == parentId).ToList();
        }

        public  List<LessonGroup> GetLessonGroupHierarchy(int lessonGroupId)
        {
            var lessonGroups = new List<LessonGroup>();
            int? currentLessonGroupId = lessonGroupId;

            while (currentLessonGroupId.HasValue)
            {
                var lessonGroup =Database.Table<LessonGroup>().Where(x => x.Id == currentLessonGroupId.Value).First();
                lessonGroups.Add(lessonGroup);
                currentLessonGroupId = lessonGroup.ParentLessonGroupId;
            }

            return lessonGroups;
        }

        public LessonGroup GetLessonGroup(int lessonGroupId)
        {
            return Database.Table<LessonGroup>().First(x => x.Id == lessonGroupId);

        }

        public List<Lesson> GetLessons(int lessonGroupId)
        {
            return Database.Table<Lesson>().Where(x => x.LessonGroupId == lessonGroupId).ToList();
        }

        public  List<VideoDownload> GetVideoDownloads()
        {
            return Database.Table<VideoDownload>().OrderBy(x => x.LessonName).ToList();
        }


        public VideoDownload GetVideoDownloadForLesson(int lessonId)
        {

            return Database.Table<VideoDownload>().FirstOrDefault(x => x.LessonId == lessonId);
        }

        public void DeleteVideoDownload(VideoDownload videoDownload)
        {
           Database.Delete<VideoDownload>(videoDownload.Id);
        }

        public void DeleteAllVideoDownloads()
        {
           Database.DeleteAll<VideoDownload>();
        }

        public void SaveVideoDownload(VideoDownload videoDownload)
        {
           Database.InsertOrReplace(videoDownload);
        }

        public  Lesson GetLesson(int lessonId)
        {
            return Database.Table<Lesson>().FirstOrDefault(x => x.Id == lessonId);
        }
        public void SaveLessonGroups(List<LessonGroup> lessongroups)
        {
           Database.DeleteAll<LessonGroup>();
           Database.InsertAll(lessongroups);
        }

        public void SaveLessons(List<Lesson> lessons)
        {
           Database.DeleteAll<Lesson>();
           Database.InsertAll(lessons);

        }

        public void SaveLesson(Lesson lesson)
        {
           Database.InsertOrReplace(lesson);
        }
    }
}
