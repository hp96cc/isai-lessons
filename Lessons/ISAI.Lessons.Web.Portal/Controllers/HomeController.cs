using System.Threading.Tasks;
using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{

    [Authorize]
    public class HomeController : BaseAdminController
    {
        public ActionResult Index()
        {
            return RedirectToAction("index", "dashboard");

        }

        public async Task<ActionResult> Test()
        {
            return View();

        }
    }

  

}


