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
    public class AppsController : BaseODataController
    {

        // GET: odata/App
        [EnableQuery]
        public IQueryable<App> GetApps()
        {
            return db.App;
        }



        // GET: odata/App(5)
        [EnableQuery]
        public SingleResult<App> GetApp([FromODataUri] int key)
        {
            return SingleResult.Create(db.App.Where(App => App.Id == key));
        }

        // PUT: odata/App(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, App patch)
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
                if (!db.App.Any(p => p.Id == key))
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

        // POST: odata/App
        public async Task<IHttpActionResult> Post(App App)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.App.Add(App);

            try
            {

                App.DateModified = DateTime.UtcNow;
                App.DateCreated = DateTime.UtcNow;
                App.CreatedUserId = User.Identity.GetUserId();
                App.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return Created(App);
        }

        // PATCH: odata/App(5)
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<App> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.App.FindAsync(key);
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
                if (!db.App.Any(p => p.Id == key))
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

        // DELETE: odata/App(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            App App = await db.App.FindAsync(key);
            if (App == null)
            {
                return NotFound();
            }

            db.App.Remove(App);
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

        private bool AppExists(int key)
        {
            return db.App.Count(e => e.Id == key) > 0;
        }
    }
}
