using ISAI.Lessons.EntityFramework.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface ISqliteService
    {

        Task<List<LessonGroup>> GetLessonGroupsAsync(int? parentId = null);

        Task<LessonGroup> GetLessonGroupAsync(int lessonGroupId);

        Task SaveLessonGroupsAsync(List<LessonGroup> lessongroups);

        Task<List<Lesson>> GetLessonsAsync(int lessonGroupId);

        Task<Lesson> GetLessonAsync(int lessonId);

        Task SaveLessonsAsync(List<Lesson> lessons);

        Task SaveLessonAsync(Lesson lesson);
      
        Task DeleteDatabaseAsync();

    }

}