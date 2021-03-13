using Microsoft.AspNet.Identity;
using Microsoft.AspNet.OData;
using ISAI.Lessons.EntityFramework.Interfaces;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace ISAI.Lessons.Web.Portal.Controllers.OData
{
    public class GenericController<T> : BaseODataController where T : class, IBaseInterface
    {

        public bool SoftDelete { get; set; } = false;

        // GET: odata/Agents
        [EnableQuery]
        public virtual IQueryable<T> Get()
        {
            return TableForT().Where(x => x.Deleted == false);
        }

        [EnableQuery]
        public virtual SingleResult<T> Get([FromODataUri] long key)
        {
            IQueryable<T> result = TableForT().Where(p => p.Id == key);
            var typedResult = SingleResult.Create(result);
            return typedResult;
        }


        public virtual async Task<IHttpActionResult> Post(T item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                item.DateModified = DateTime.UtcNow;
                item.DateCreated = DateTime.UtcNow;
                item.CreatedUserId = User.Identity.GetUserId();
                item.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }

            TableForT().Add(item);
            await db.SaveChangesAsync();
            return Created(item);
        }


        public async Task<IHttpActionResult> Put([FromODataUri] int key, T patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != patch.Id)
            {
                return BadRequest();
            }

            patch.DateModified = DateTime.UtcNow;
            patch.ModifiedUserId = User.Identity.GetUserId();

            db.Entry(patch).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(key))
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


        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<T> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await TableForT().FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            delta.Patch(entity);

            try
            {
                entity.DateModified = DateTime.UtcNow;
                entity.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(key))
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

        public async Task<IHttpActionResult> Put([FromODataUri] long key, T entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }

            db.Entry(entity).State = EntityState.Modified;

            try
            {

                entity.DateModified = DateTime.UtcNow;
                entity.ModifiedUserId = User.Identity.GetUserId();

                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(key))
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

        public async Task<IHttpActionResult> Delete([FromODataUri] long key)
        {
            var entity = await TableForT().FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            if (SoftDelete)
            {

                entity.Deleted = true;
                entity.DateModified = DateTime.UtcNow;
                entity.ModifiedUserId = User.Identity.GetUserId();

                db.Entry(entity).State = EntityState.Modified;

            } else
            {
                TableForT().Remove(entity);

            }

            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }


        private bool Exists(long key)
        {
            return TableForT().Any(p => p.Id == key);
        }

        private DbSet<T> TableForT()
        {
            return db.Set<T>();
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