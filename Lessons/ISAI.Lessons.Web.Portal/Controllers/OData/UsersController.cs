using Microsoft.AspNet.OData;
using ISAI.Lessons.EntityFramework.Models;
using System.Linq;
using System.Web.Http;

namespace ISAI.Lessons.Web.Portal.Controllers.OData
{

    [Authorize]
    public class UsersController : BaseODataController
    {

        // GET: odata/TaskOutput
        [EnableQuery]
        public IQueryable<User> GetUsers()
        {
            return db.Users.Where(x => x.Deleted == false);
        }


        // GET: odata/TaskOutput(5)
        [EnableQuery]
        public SingleResult<User> GetTaskOutput([FromODataUri] string key)
        {
            return SingleResult.Create(db.Users.Where(x => x.Id == key));
        }


    }

}
