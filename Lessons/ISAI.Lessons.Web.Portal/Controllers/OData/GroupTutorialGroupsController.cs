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
    public class GroupTutorialGroupsController : BaseODataController
    {

        [EnableQuery]
        public IQueryable<GroupTutorialGroup> GetGroupTutorialGroups()
        {
            return db.GroupTutorialGroup.Where(x => x.Deleted == false);
        }



        [EnableQuery]
        public SingleResult<GroupTutorialGroup> GetGroupTutorialGroup([FromODataUri] int key)
        {
            return SingleResult.Create(db.GroupTutorialGroup.Where(GroupTutorialGroup => GroupTutorialGroup.Id == key));
        }

        public async Task<IHttpActionResult> Put([FromODataUri] int key, GroupTutorialGroup patch)
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
                if (!db.GroupTutorialGroup.Any(p => p.Id == key))
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

        public async Task<IHttpActionResult> Post(GroupTutorialGroup GroupTutorialGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.GroupTutorialGroup.Add(GroupTutorialGroup);

            try
            {

                GroupTutorialGroup.DateModified = DateTime.UtcNow;
                GroupTutorialGroup.DateCreated = DateTime.UtcNow;
                GroupTutorialGroup.CreatedUserId = User.Identity.GetUserId();
                GroupTutorialGroup.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(GroupTutorialGroup);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<GroupTutorialGroup> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.GroupTutorialGroup.FindAsync(key);
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
                if (!db.GroupTutorialGroup.Any(p => p.Id == key))
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
            GroupTutorialGroup GroupTutorialGroup = await db.GroupTutorialGroup.FindAsync(key);
            if (GroupTutorialGroup == null)
            {
                return NotFound();
            }

            GroupTutorialGroup.Deleted = true;
            GroupTutorialGroup.DateModified = DateTime.UtcNow;
            GroupTutorialGroup.ModifiedUserId = User.Identity.GetUserId();

            db.Entry(GroupTutorialGroup).State = EntityState.Modified;
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
