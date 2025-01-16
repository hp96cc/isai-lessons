using ISAI.Lessons.AppModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface ISqliteService
    {

        Task InitializeAsync();

        Task<AppUser> GetUserAsync();

        Task DeleteUserAsync(AppUser appUser);

        Task SaveUserAsync(AppUser appUser);

        Task<List<LessonGroup>> GetLessonGroupsAsync(int? parentId = null);

        Task<List<LessonGroup>> GetLessonGroupHierarchyAsync(int lessonGroupId);

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