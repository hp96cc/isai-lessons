
using Microsoft.AspNet.OData;

namespace ISAI.Lessons.Web.Portal.Controllers.ODataReports
{
   public class BaseODataReportController : ODataController
    {

        protected ISAI.Lessons.EntityFramework.Reports.LessonReports db = new ISAI.Lessons.EntityFramework.Reports.LessonReports();

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