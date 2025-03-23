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
    public class TutorialsController : BaseODataController
    {

        [EnableQuery]
        public IQueryable<Tutorial> GetTutorials()
        {
            return db.Tutorial.Where(x => x.Deleted == false);
        }



        [EnableQuery]
        public SingleResult<Tutorial> GetTutorial([FromODataUri] int key)
        {
            return SingleResult.Create(db.Tutorial.Where(Tutorial => Tutorial.Id == key));
        }

        public async Task<IHttpActionResult> Put([FromODataUri] int key, Tutorial patch)
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
                if (!db.Tutorial.Any(p => p.Id == key))
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

        public async Task<IHttpActionResult> Post(Tutorial Tutorial)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Tutorial.Add(Tutorial);

            try
            {

                Tutorial.DateModified = DateTime.UtcNow;
                Tutorial.DateCreated = DateTime.UtcNow;
                Tutorial.CreatedUserId = User.Identity.GetUserId();
                Tutorial.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(Tutorial);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Tutorial> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Tutorial.FindAsync(key);
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
                if (!db.Tutorial.Any(p => p.Id == key))
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
            Tutorial Tutorial = await db.Tutorial.FindAsync(key);
            if (Tutorial == null)
            {
                return NotFound();
            }

            Tutorial.Deleted = true;
            Tutorial.DateModified = DateTime.UtcNow;
            Tutorial.ModifiedUserId = User.Identity.GetUserId();

            db.Entry(Tutorial).State = EntityState.Modified;
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
