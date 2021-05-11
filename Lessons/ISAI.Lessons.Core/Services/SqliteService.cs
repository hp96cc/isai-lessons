using ISAI.Lessons.Core.Extensions;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.Core.Services
{
    public class SqliteService : ISqliteService
    {
        public const string DatabaseFilename = "Lessons_v8.db3";

        public const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }

        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(DatabasePath, Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;

        static bool initialized = false;

        public SqliteService()
        {
            //InitializeAsync().SafeFireAndForget(false);
        }

        public async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(LessonGroup).Name))
                {
                    await Database.CreateTableAsync<LessonGroup>(CreateFlags.None).ConfigureAwait(false);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Lesson).Name))
                {
                    await Database.CreateTableAsync<Lesson>(CreateFlags.None).ConfigureAwait(false);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(VideoDownload).Name))
                {
                    await Database.CreateTableAsync<VideoDownload>(CreateFlags.None).ConfigureAwait(false);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(AppUser).Name))
                {
                    await Database.CreateTableAsync<AppUser>(CreateFlags.None).ConfigureAwait(false);
                }

                initialized = true;
            }

        }


        public async Task DeleteDatabaseAsync()
        {

            await Database.DeleteAllAsync<LessonGroup>().ConfigureAwait(false);
            await Database.DeleteAllAsync<Lesson>().ConfigureAwait(false);
            await Database.DeleteAllAsync<VideoDownload>().ConfigureAwait(false);
            await Database.DeleteAllAsync<AppUser>().ConfigureAwait(false);

            Console.WriteLine("Database deleted");

        }

        public async Task<AppUser> GetUserAsync()
        {
            return await Database.Table<AppUser>().FirstOrDefaultAsync();
        }

        public async Task DeleteUserAsync(AppUser appUser)
        {
            await Database.DeleteAsync<AppUser>(appUser.Id);
        }

        public async Task SaveUserAsync(AppUser appUser)
        {
            await Database.InsertOrReplaceAsync(appUser);
        }

        public async Task<List<LessonGroup>> GetLessonGroupsAsync(int? parentId = null)
        {
            return await Database.Table<LessonGroup>().Where(x => x.ParentLessonGroupId == parentId).ToListAsync();
        }

        public async Task<List<LessonGroup>> GetLessonGroupHierarchyAsync(int lessonGroupId)
        {
            var lessonGroups = new List<LessonGroup>();
            int? currentLessonGroupId = lessonGroupId;

            while (currentLessonGroupId.HasValue)
            {
                var lessonGroup = await Database.Table<LessonGroup>().Where(x => x.Id == currentLessonGroupId.Value).FirstAsync();
                lessonGroups.Add(lessonGroup);
                currentLessonGroupId = lessonGroup.ParentLessonGroupId;
            }

            return lessonGroups;
        }

        public async Task<LessonGroup> GetLessonGroupAsync(int lessonGroupId)
        {
            return await Database.Table<LessonGroup>().FirstAsync(x => x.Id == lessonGroupId);

        }

        public async Task<List<Lesson>> GetLessonsAsync(int lessonGroupId)
        {
            return await Database.Table<Lesson>().Where(x => x.LessonGroupId == lessonGroupId).ToListAsync();
        }

        public async Task<List<VideoDownload>> GetVideoDownloadsAsync()
        {
            return await Database.Table<VideoDownload>().OrderBy(x => x.LessonName).ToListAsync();
        }

        public async Task<VideoDownload> GetVideoDownloadForLessonAsync(int lessonId) {

            return await Database.Table<VideoDownload>().FirstOrDefaultAsync(x => x.LessonId == lessonId);
        }

        public async Task DeleteVideoDownloadAsync(VideoDownload videoDownload)
        {
            await Database.DeleteAsync<VideoDownload>(videoDownload.Id);
        }

        public async Task DeleteAllVideoDownloadsAsync()
        {
            await Database.DeleteAllAsync<VideoDownload>().ConfigureAwait(false);
        }

        public async Task SaveVideoDownloadAsync(VideoDownload videoDownload)
        {
            await Database.InsertOrReplaceAsync(videoDownload);
        }

        public async Task<Lesson> GetLessonAsync(int lessonId)
        {
            return await Database.Table<Lesson>().FirstOrDefaultAsync(x => x.Id == lessonId);
        }
        public async Task SaveLessonGroupsAsync(List<LessonGroup> lessongroups)
        {
            await Database.DeleteAllAsync<LessonGroup>();
            await Database.InsertAllAsync(lessongroups);
        }

        public async Task SaveLessonsAsync(List<Lesson> lessons)
        {
            await Database.DeleteAllAsync<Lesson>();
            await Database.InsertAllAsync(lessons);

        }

        public async Task SaveLessonAsync(Lesson lesson)
        {
            await Database.InsertOrReplaceAsync(lesson);
        }
    }
}
