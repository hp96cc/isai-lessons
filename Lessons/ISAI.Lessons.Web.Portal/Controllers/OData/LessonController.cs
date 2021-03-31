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
    public class LessonsController : BaseODataController
    {

        // GET: odata/Lesson
        [EnableQuery]
        public IQueryable<Lesson> GetLessons()
        {
            return db.Lesson.Where(x => x.Deleted == false);
        }



        // GET: odata/Lesson(5)
        [EnableQuery]
        public SingleResult<Lesson> GetLesson([FromODataUri] int key)
        {
            return SingleResult.Create(db.Lesson.Where(Lesson => Lesson.Id == key));
        }

        // PUT: odata/Lesson(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Lesson patch)
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
                if (!db.Lesson.Any(p => p.Id == key))
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

        // POST: odata/Lesson
        public async Task<IHttpActionResult> Post(Lesson Lesson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Lesson.Add(Lesson);

            try
            {

                Lesson.DateModified = DateTime.UtcNow;
                Lesson.DateCreated = DateTime.UtcNow;
                Lesson.CreatedUserId = User.Identity.GetUserId();
                Lesson.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(Lesson);
        }

        // PATCH: odata/Lesson(5)
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Lesson> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Lesson.FindAsync(key);
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
                if (!db.Lesson.Any(p => p.Id == key))
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
            Lesson Lesson = await db.Lesson.FindAsync(key);
            if (Lesson == null)
            {
                return NotFound();
            }

            db.Lesson.Remove(Lesson);
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("odata/lessons/sortlessons")]
        [HttpGet]
        public async Task SortLessons(int dropIndex, int rowId, int appId)
        {
            var movedRow = await db.Lesson.FirstAsync(x => x.Id == rowId && x.AppId == appId);

            var rows = await db.Lesson
                .Where(x => x.LessonGroupId == movedRow.LessonGroupId && x.Deleted == false && x.AppId == appId)
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

        private bool LessonExists(int key)
        {
            return db.Lesson.Count(e => e.Id == key) > 0;
        }
    }
}
