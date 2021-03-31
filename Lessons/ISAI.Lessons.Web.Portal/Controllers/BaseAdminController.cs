using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ISAI.Lessons.EntityFramework;
using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.Web.Portal.Helpers;

namespace ISAI.Lessons.Web.Portal
{

    public abstract class BaseAdminController : Controller
    {

        protected LessonsDbContext db = new LessonsDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            ViewBag.Title = "Scottish Online Lessons";

        }

        protected async Task GetParentLessonGroup(int parentLessonGroupId, List<LessonGroup> lessonGroups)
        {
            var parentLessonGroup = await db.LessonGroup.FirstAsync(x => x.Id == parentLessonGroupId);
            lessonGroups.Add(parentLessonGroup);

            if (parentLessonGroup.ParentLessonGroupId != null)
            {
                await GetParentLessonGroup(parentLessonGroup.ParentLessonGroupId.Value, lessonGroups);
            }

        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}