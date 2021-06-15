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
    public class LessonGroupsController : BaseODataController
    {

        // GET: odata/LessonGroup
        [EnableQuery]
        public IQueryable<LessonGroup> GetLessonGroups()
        {
            return db.LessonGroup.Where(x => x.Deleted == false);
        }



        // GET: odata/LessonGroup(5)
        [EnableQuery]
        public SingleResult<LessonGroup> GetLessonGroup([FromODataUri] int key)
        {
            return SingleResult.Create(db.LessonGroup.Where(LessonGroup => LessonGroup.Id == key));
        }

        // PUT: odata/LessonGroup(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, LessonGroup patch)
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
                if (!db.LessonGroup.Any(p => p.Id == key))
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

        // POST: odata/LessonGroup
        public async Task<IHttpActionResult> Post(LessonGroup LessonGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.LessonGroup.Add(LessonGroup);

            try
            {
                if (LessonGroup.ParentLessonGroupId != null)
                {
                    var lessonParentGroup = await db.LessonGroup.FirstAsync(x => x.Id == LessonGroup.ParentLessonGroupId);
                    LessonGroup.SubscriptionTypeId = lessonParentGroup.SubscriptionTypeId;
                }

                LessonGroup.DateModified = DateTime.UtcNow;
                LessonGroup.DateCreated = DateTime.UtcNow;
                LessonGroup.CreatedUserId = User.Identity.GetUserId();
                LessonGroup.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(LessonGroup);
        }

        // PATCH: odata/LessonGroup(5)
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<LessonGroup> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.LessonGroup.FindAsync(key);
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
                if (!db.LessonGroup.Any(p => p.Id == key))
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

        // DELETE: odata/LessonGroup(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            LessonGroup LessonGroup = await db.LessonGroup.FindAsync(key);
            if (LessonGroup == null)
            {
                return NotFound();
            }

            db.LessonGroup.Remove(LessonGroup);
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }


        [Route("odata/lessongroups/sortlessongroup")]
        [HttpGet]
        public async Task SortGroupOption(int dropIndex, int rowId, int appId)
        {
            var movedRow = await db.LessonGroup.FirstAsync(x => x.Id == rowId && x.AppId == appId);

            var rows = await db.LessonGroup
                .Where(x => x.ParentLessonGroupId == movedRow.ParentLessonGroupId && x.Deleted == false && x.AppId == appId)
                .OrderBy(x => x.ListOrder)
                .ToListAsync();

            int listOrder = 0;

            rows.Remove(movedRow);
            rows.Insert(dropIndex, movedRow);


            foreach (var row in rows)
            {
                row.ListOrder = listOrder;
                db.Entry(row).State = EntityState.Modified;
                listOrder++;
            }

            await db.SaveChangesAsync();

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LessonGroupExists(int key)
        {
            return db.LessonGroup.Count(e => e.Id == key) > 0;
        }
    }
}
