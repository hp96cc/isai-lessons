using System.Threading.Tasks;
using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{

    [Authorize]
    public class AppController : BaseAuthroizationController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.ItemName = "App";

        }

        // GET: /Subjects/
        public async Task<ActionResult> Index()
        {


            return View();
        }




    }
}