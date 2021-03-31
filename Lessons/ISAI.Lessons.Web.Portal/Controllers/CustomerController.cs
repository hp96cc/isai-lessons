using System.Threading.Tasks;
using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{

    [Authorize]
    public class CustomerController : BaseAuthroizationController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.ItemName = "Customer";

        }

        public ActionResult Index()
        {
            return View();
        }




    }
}