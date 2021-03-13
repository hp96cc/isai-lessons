using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{
    public class DashboardController : Controller
    {
       
        public ActionResult Index()
        {
            return Redirect("/order/");
        }


    }
}