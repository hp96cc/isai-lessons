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
    public class SubscriptionTypesController : BaseODataController
    {

        // GET: odata/SubscriptionType
        [EnableQuery]
        public IQueryable<SubscriptionType> GetSubscriptionTypes()
        {
            return db.SubscriptionType.Where(x => x.Deleted == false);
        }



        // GET: odata/SubscriptionType(5)
        [EnableQuery]
        public SingleResult<SubscriptionType> GetSubscriptionType([FromODataUri] int key)
        {
            return SingleResult.Create(db.SubscriptionType.Where(SubscriptionType => SubscriptionType.Id == key));
        }

        // PUT: odata/SubscriptionType(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, SubscriptionType patch)
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
                if (!db.SubscriptionType.Any(p => p.Id == key))
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

        // POST: odata/SubscriptionType
        public async Task<IHttpActionResult> Post(SubscriptionType SubscriptionType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.SubscriptionType.Add(SubscriptionType);

            try
            {

                SubscriptionType.DateModified = DateTime.UtcNow;
                SubscriptionType.DateCreated = DateTime.UtcNow;
                SubscriptionType.CreatedUserId = User.Identity.GetUserId();
                SubscriptionType.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(SubscriptionType);
        }

        // PATCH: odata/SubscriptionType(5)
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<SubscriptionType> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.SubscriptionType.FindAsync(key);
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
                if (!db.SubscriptionType.Any(p => p.Id == key))
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

        // DELETE: odata/SubscriptionType(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            SubscriptionType SubscriptionType = await db.SubscriptionType.FindAsync(key);
            if (SubscriptionType == null)
            {
                return NotFound();
            }

            db.SubscriptionType.Remove(SubscriptionType);
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

        private bool SubscriptionTypeExists(int key)
        {
            return db.SubscriptionType.Count(e => e.Id == key) > 0;
        }
    }
}
