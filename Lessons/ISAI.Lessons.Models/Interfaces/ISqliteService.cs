using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface ISqliteService
    {

        Task<AppUser> GetUserAsync();

        Task DeleteUserAsync(AppUser appUser);

        Task SaveUserAsync(AppUser appUser);

        Task<List<LessonGroup>> GetLessonGroupsAsync(int? parentId = null);

        Task<LessonGroup> GetLessonGroupAsync(int lessonGroupId);

        Task SaveLessonGroupsAsync(List<LessonGroup> lessongroups);

        Task<List<Lesson>> GetLessonsAsync(int lessonGroupId);

        Task<List<VideoDownload>> GetVideoDownloadsAsync();

        Task<VideoDownload> GetVideoDownloadForLessonAsync(int lessonId);

        Task DeleteVideoDownloadAsync(VideoDownload videoDownload);

        Task DeleteAllVideoDownloadsAsync();

        Task SaveVideoDownloadAsync(VideoDownload videoDownload);

        Task<Lesson> GetLessonAsync(int lessonId);

        Task SaveLessonsAsync(List<Lesson> lessons);

        Task SaveLessonAsync(Lesson lesson);
      
        Task DeleteDatabaseAsync();

    }

}