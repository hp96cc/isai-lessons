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
using Microsoft.AspNet.OData.Routing;

namespace ISAI.Lessons.Web.Portal.Controllers.OData
{


    [Authorize]
    public class SubscriptionCodesController : BaseODataController
    {

        // GET: odata/Subscription
        [EnableQuery]
        public IQueryable<SubscriptionCode> GetSubscriptionCodes()
        {
            return db.SubscriptionCode.Where(x => x.Deleted == false);
        }



        // GET: odata/Subscription(5)
        [EnableQuery]
        public SingleResult<SubscriptionCode> GetSubscription([FromODataUri] int key)
        {
            return SingleResult.Create(db.SubscriptionCode.Where(SubscriptionCode => SubscriptionCode.Id == key));
        }

        // PUT: odata/Subscription(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, SubscriptionCode patch)
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
                if (!db.SubscriptionCode.Any(p => p.Id == key))
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

        public async Task<IHttpActionResult> Post(SubscriptionCode SubscriptionCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //HACK: to allow the multiple creataion of codes
            var quantity = SubscriptionCode.Id;

            for (int i = 0; i < quantity; i++)
            {

                try
                {

                    var subscriptionCode = new SubscriptionCode()
                    {
                        Code = Guid.NewGuid().ToString(),
                        Deleted = false,
                        IssuedTo = SubscriptionCode.IssuedTo,
                        LicenceDays = SubscriptionCode.LicenceDays,
                        LicenceExpiryDate = SubscriptionCode.LicenceExpiryDate,
                        SubscriptionTypeId = SubscriptionCode.SubscriptionTypeId,
                        ValidFrom = SubscriptionCode.ValidFrom,
                        ValidTo = SubscriptionCode.ValidTo,
                        DateModified = DateTime.UtcNow,
                        DateCreated = DateTime.UtcNow,
                        CreatedUserId = User.Identity.GetUserId(),
                        ModifiedUserId = User.Identity.GetUserId()
                    };

                    db.SubscriptionCode.Add(subscriptionCode);

                    await db.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }


            return Created(SubscriptionCode);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<SubscriptionCode> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.SubscriptionCode.FindAsync(key);
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
                if (!db.SubscriptionCode.Any(p => p.Id == key))
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
            SubscriptionCode SubscriptionCode = await db.SubscriptionCode.FindAsync(key);
            if (SubscriptionCode == null)
            {
                return NotFound();
            }

            db.SubscriptionCode.Remove(SubscriptionCode);
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
            return db.SubscriptionCode.Count(e => e.Id == key) > 0;
        }
    }
}
