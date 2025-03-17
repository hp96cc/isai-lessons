using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{

    [Authorize]
    public class GroupTutorialController : BaseAuthroizationController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.ItemName = "Group Tutorial";

        }

        public ActionResult Index()
        {
            return View();
        }




    }
}