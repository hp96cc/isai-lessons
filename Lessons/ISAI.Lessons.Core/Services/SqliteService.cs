using ISAI.Lessons.Core.Extensions;
using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.Models.Interfaces;
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
        public const string DatabaseFilename = "Lessons_v2.db3";

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
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
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


                initialized = true;
            }

        }


        public async Task DeleteDatabaseAsync()
        {

            await Database.DeleteAllAsync<LessonGroup>().ConfigureAwait(false);
            await Database.DeleteAllAsync<Lesson>().ConfigureAwait(false);

            Console.WriteLine("Database deleted");

        }


        public async Task<List<LessonGroup>> GetLessonGroupsAsync(int? parentId = null)
        {
            return await Database.Table<LessonGroup>().Where(x => x.ParentLessonGroupId == parentId).ToListAsync();
        }

        public async Task<LessonGroup> GetLessonGroupAsync(int lessonGroupId)
        {
            return await Database.Table<LessonGroup>().FirstAsync(x => x.Id == lessonGroupId);

        }

        public async Task<List<Lesson>> GetLessonsAsync(int lessonGroupId)
        {
            return await Database.Table<Lesson>().Where(x => x.LessonGroupId == lessonGroupId).ToListAsync();
        }

        public async Task<Lesson> GetLessonAsync(int lessonId)
        {
            return await Database.Table<Lesson>().FirstAsync(x => x.Id == lessonId);

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
