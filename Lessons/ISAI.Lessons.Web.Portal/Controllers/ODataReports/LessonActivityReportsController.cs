using Microsoft.AspNet.Identity;
using Microsoft.AspNet.OData;
using ISAI.Lessons.EntityFramework.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ISAI.Lessons.Web.Portal.Controllers.OData;
using ISAI.Lessons.Web.Portal.Helpers;
using ISAI.Lessons.EntityFramework.Reports;

namespace ISAI.Lessons.Web.Portal.Controllers.ODataReports
{


    [Authorize]
    public class LessonActivityReportsController : BaseODataReportController
    {

        [EnableQuery]
        public IQueryable<LessonActivityReport> GetLessonActivityReports()
        {

            return db.LessonActivityReports;

        }

    }
}
