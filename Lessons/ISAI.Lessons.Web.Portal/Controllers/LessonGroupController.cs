using ISAI.Lessons.EntityFramework.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{

    [Authorize]
    public class LessonGroupController : BaseAdminController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.ItemName = "Lesson Group";

        }

        public async Task<ActionResult> Index()
        {

            var breadCrumbList = new List<LessonGroup>();

            if(Request.QueryString["parentLessonGroupId"] != null)
            {
                var parentLessonGroupId = Convert.ToInt32(Request.QueryString["parentLessonGroupId"]);
                await GetParentLessonGroup(parentLessonGroupId, breadCrumbList);

            }

            breadCrumbList.Reverse();

            ViewBag.BreadCrumbList = breadCrumbList;

            return View();
        }

        
        

    }
}