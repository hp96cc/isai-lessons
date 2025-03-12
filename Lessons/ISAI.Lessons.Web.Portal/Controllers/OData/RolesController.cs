using Microsoft.AspNet.OData;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ISAI.Lessons.Web.Portal.Controllers.OData
{


    [Authorize]
    public class RolesController : BaseODataController
    {
        [EnableQuery]
        public IQueryable<IdentityRole> GetRoles()
        {
            return db.Roles;
        }

    }
}
