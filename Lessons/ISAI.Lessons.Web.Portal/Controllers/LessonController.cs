using ISAI.Lessons.EntityFramework.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{

    [Authorize]
    public class LessonController : BaseAdminController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.ItemName = "Lesson";

        }

        public async Task<ActionResult> Index()
        {

            var breadCrumbList = new List<LessonGroup>();

            if (Request.QueryString["lessonGroupId"] != null)
            {
                var lessonGroupId = Convert.ToInt32(Request.QueryString["lessonGroupId"]);
                await GetParentLessonGroup(lessonGroupId, breadCrumbList);

            }

            breadCrumbList.Reverse();

            ViewBag.BreadCrumbList = breadCrumbList;

            return View();
        }

    }
}