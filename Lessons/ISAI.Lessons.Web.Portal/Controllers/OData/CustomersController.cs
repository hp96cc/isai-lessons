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
    public class CustomersController : BaseODataController
    {

        // GET: odata/Customer
        [EnableQuery]
        public IQueryable<Customer> GetCustomers()
        {
            return db.Customer.Where(x => x.Deleted == false);
        }



        // GET: odata/Customer(5)
        [EnableQuery]
        public SingleResult<Customer> GetCustomer([FromODataUri] int key)
        {
            return SingleResult.Create(db.Customer.Where(Customer => Customer.Id == key));
        }

        // PUT: odata/Customer(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Customer patch)
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
                if (!db.Customer.Any(p => p.Id == key))
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

        // POST: odata/Customer
        public async Task<IHttpActionResult> Post(Customer Customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Customer.Add(Customer);

            try
            {

                Customer.DateModified = DateTime.UtcNow;
                Customer.DateCreated = DateTime.UtcNow;
                Customer.CreatedUserId = User.Identity.GetUserId();
                Customer.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(Customer);
        }

        // PATCH: odata/Customer(5)
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Customer> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Customer.FindAsync(key);
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
                if (!db.Customer.Any(p => p.Id == key))
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

        // DELETE: odata/Customer(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            Customer Customer = await db.Customer.FindAsync(key);
            if (Customer == null)
            {
                return NotFound();
            }

            db.Customer.Remove(Customer);
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

        private bool CustomerExists(int key)
        {
            return db.Customer.Count(e => e.Id == key) > 0;
        }
    }
}
