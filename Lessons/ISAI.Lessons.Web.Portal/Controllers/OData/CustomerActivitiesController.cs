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
    public class CustomerActivitiesController : BaseODataController
    {

        [EnableQuery]
        public IQueryable<CustomerActivity> GetCustomerActivities()
        {
            return db.CustomerActivity.Where(x => x.Deleted == false);
        }



        [EnableQuery]
        public SingleResult<CustomerActivity> GetCustomerActivity([FromODataUri] int key)
        {
            return SingleResult.Create(db.CustomerActivity.Where(CustomerActivity => CustomerActivity.Id == key));
        }

        public async Task<IHttpActionResult> Put([FromODataUri] int key, CustomerActivity patch)
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
                if (!db.CustomerActivity.Any(p => p.Id == key))
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

        public async Task<IHttpActionResult> Post(CustomerActivity CustomerActivity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.CustomerActivity.Add(CustomerActivity);

            try
            {

                CustomerActivity.DateModified = DateTime.UtcNow;
                CustomerActivity.DateCreated = DateTime.UtcNow;
                CustomerActivity.CreatedUserId = User.Identity.GetUserId();
                CustomerActivity.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(CustomerActivity);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<CustomerActivity> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.CustomerActivity.FindAsync(key);
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
                if (!db.CustomerActivity.Any(p => p.Id == key))
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
            CustomerActivity CustomerActivity = await db.CustomerActivity.FindAsync(key);
            if (CustomerActivity == null)
            {
                return NotFound();
            }

            CustomerActivity.Deleted = true;
            CustomerActivity.DateModified = DateTime.UtcNow;
            CustomerActivity.ModifiedUserId = User.Identity.GetUserId();

            db.Entry(CustomerActivity).State = EntityState.Modified;
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
