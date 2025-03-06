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
    public class TutorialSubjectsController : BaseODataController
    {

        [EnableQuery]
        public IQueryable<TutorialSubject> GetTutorialSubjects()
        {
            return db.TutorialSubject.Where(x => x.Deleted == false);
        }


        [EnableQuery]
        public SingleResult<TutorialSubject> GetTutorialSubject([FromODataUri] int key)
        {
            return SingleResult.Create(db.TutorialSubject.Where(TutorialSubject => TutorialSubject.Id == key));
        }


        public async Task<IHttpActionResult> Put([FromODataUri] int key, TutorialSubject patch)
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
                if (!db.TutorialSubject.Any(p => p.Id == key))
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


        public async Task<IHttpActionResult> Post(TutorialSubject TutorialSubject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.TutorialSubject.Add(TutorialSubject);

            try
            {

                TutorialSubject.DateModified = DateTime.UtcNow;
                TutorialSubject.DateCreated = DateTime.UtcNow;
                TutorialSubject.CreatedUserId = User.Identity.GetUserId();
                TutorialSubject.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(TutorialSubject);
        }


        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<TutorialSubject> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.TutorialSubject.FindAsync(key);
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
                if (!db.TutorialSubject.Any(p => p.Id == key))
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
            TutorialSubject TutorialSubject = await db.TutorialSubject.FindAsync(key);
            if (TutorialSubject == null)
            {
                return NotFound();
            }

            db.TutorialSubject.Remove(TutorialSubject);
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
