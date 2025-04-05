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

namespace ISAI.Lessons.Web.Portal.Controllers.OData
{


    [Authorize]
    public class GroupTutorialsController : BaseODataController
    {

        [EnableQuery]
        public IQueryable<GroupTutorial> GetGroupTutorials()
        {
            return db.GroupTutorial.Where(x => x.Deleted == false);
        }



        [EnableQuery]
        public SingleResult<GroupTutorial> GetGroupTutorial([FromODataUri] int key)
        {
            return SingleResult.Create(db.GroupTutorial.Where(GroupTutorial => GroupTutorial.Id == key));
        }

        public async Task<IHttpActionResult> Put([FromODataUri] int key, GroupTutorial patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != patch.Id)
            {
                return BadRequest();
            }
            db.Entry(patch).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!db.GroupTutorial.Any(p => p.Id == key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(patch);
        }

        public async Task<IHttpActionResult> Post(GroupTutorial GroupTutorial)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GroupTutorial.TutorUser = null;

            db.GroupTutorial.Add(GroupTutorial);

            try
            {

                GroupTutorial.DateModified = DateTime.UtcNow;
                GroupTutorial.DateCreated = DateTime.UtcNow;
                GroupTutorial.CreatedUserId = User.Identity.GetUserId();
                GroupTutorial.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(GroupTutorial);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<GroupTutorial> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.GroupTutorial.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            patch.Patch(entity);

            entity.DateModified = DateTime.UtcNow;
            entity.ModifiedUserId = User.Identity.GetUserId();

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!db.GroupTutorial.Any(p => p.Id == key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(entity);
        }

        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            GroupTutorial GroupTutorial = await db.GroupTutorial.FindAsync(key);
            if (GroupTutorial == null)
            {
                return NotFound();
            }

            GroupTutorial.Deleted = true;
            GroupTutorial.DateModified = DateTime.UtcNow;
            GroupTutorial.ModifiedUserId = User.Identity.GetUserId();

            db.Entry(GroupTutorial).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

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
