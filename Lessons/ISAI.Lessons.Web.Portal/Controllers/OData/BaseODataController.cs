using Microsoft.AspNet.OData;
using ISAI.Lessons.EntityFramework;

namespace ISAI.Lessons.Web.Portal.Controllers.OData
{
   public class BaseODataController : ODataController
    {

        protected LessonsDbContext db = new LessonsDbContext();

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