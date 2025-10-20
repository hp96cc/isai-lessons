using ISAI.Lessons.Models.Models.App;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISAI.Lessons.Models.Interfaces.App
{
    public interface ISqliteService
    {

        void Initialize();

       AppUser GetUser();

        void DeleteUser(AppUser appUser);

        void SaveUser(AppUser appUser);

        List<LessonGroup> GetLessonGroups(int? parentId = null);

        List<LessonGroup> GetLessonGroupHierarchy(int lessonGroupId);

        LessonGroup GetLessonGroup(int lessonGroupId);

        void SaveLessonGroups(List<LessonGroup> lessongroups);

        List<Lesson> GetLessons(int lessonGroupId);

        List<VideoDownload> GetVideoDownloads();

        VideoDownload GetVideoDownloadForLesson(int lessonId);

        void DeleteVideoDownload(VideoDownload videoDownload);

        void DeleteAllVideoDownloads();

        void SaveVideoDownload(VideoDownload videoDownload);

        Lesson GetLesson(int lessonId);

        void SaveLessons(List<Lesson> lessons);

        void SaveLesson(Lesson lesson);

        void DeleteDatabase();

    }
}
