using System.Web.Http;
using ISAI.Lessons.EntityFramework;

namespace ISAI.Lessons.Web.Portal.Controllers.Api
{
    public class BaseApiController : ApiController
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

