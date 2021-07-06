using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{

    [Authorize]
    public class SubscriptionController : BaseAuthroizationController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.ItemName = "Subscription";

        }

        public ActionResult Index()
        {
            return View();
        }




    }
}