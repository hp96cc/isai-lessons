using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{

    [Authorize]
    public class ReportController : BaseAuthroizationController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.ItemName = "Report";

        }

        public ActionResult LessonActivityReport()
        {
            ViewBag.ItemName = "Lesson Activity Report";
            return View();
        }

    }
}