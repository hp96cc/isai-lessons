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
    public class SubscriptionsController : BaseODataController
    {

        // GET: odata/Subscription
        [EnableQuery]
        public IQueryable<Subscription> GetSubscriptions()
        {
            return db.Subscription.Where(x => x.Deleted == false);
        }



        // GET: odata/Subscription(5)
        [EnableQuery]
        public SingleResult<Subscription> GetSubscription([FromODataUri] int key)
        {
            return SingleResult.Create(db.Subscription.Where(Subscription => Subscription.Id == key));
        }

        // PUT: odata/Subscription(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Subscription patch)
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
                if (!db.Subscription.Any(p => p.Id == key))
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

        // POST: odata/Subscription
        public async Task<IHttpActionResult> Post(Subscription Subscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Subscription.Add(Subscription);

            try
            {

                Subscription.DateModified = DateTime.UtcNow;
                Subscription.DateCreated = DateTime.UtcNow;
                Subscription.CreatedUserId = User.Identity.GetUserId();
                Subscription.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(Subscription);
        }

        // PATCH: odata/Subscription(5)
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Subscription> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Subscription.FindAsync(key);
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
                if (!db.Subscription.Any(p => p.Id == key))
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

        // DELETE: odata/Subscription(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            Subscription Subscription = await db.Subscription.FindAsync(key);
            if (Subscription == null)
            {
                return NotFound();
            }

            db.Subscription.Remove(Subscription);
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

        private bool SubscriptionExists(int key)
        {
            return db.Subscription.Count(e => e.Id == key) > 0;
        }
    }
}
