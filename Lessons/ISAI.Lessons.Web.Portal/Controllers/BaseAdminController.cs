using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ISAI.Lessons.EntityFramework;
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