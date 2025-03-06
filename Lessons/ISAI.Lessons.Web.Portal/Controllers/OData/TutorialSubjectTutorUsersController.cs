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
        public class TutorialSubjectTutorUsersController : BaseODataController
        {

            [EnableQuery]
            public IQueryable<TutorialSubjectTutorUser> GetTutorialSubjectTutorUsers()
            {
                return db.TutorialSubjectTutorUser;
            }


            [EnableQuery]
            public SingleResult<TutorialSubjectTutorUser> GetTutorialSubjectTutorUser([FromODataUri] int key)
            {
                return SingleResult.Create(db.TutorialSubjectTutorUser.Where(TutorialSubjectTutorUser => TutorialSubjectTutorUser.Id == key));
            }


            public async Task<IHttpActionResult> Put([FromODataUri] int key, TutorialSubjectTutorUser patch)
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
                    if (!db.TutorialSubjectTutorUser.Any(p => p.Id == key))
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


            public async Task<IHttpActionResult> Post(TutorialSubjectTutorUser TutorialSubjectTutorUser)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                db.TutorialSubjectTutorUser.Add(TutorialSubjectTutorUser);

                try
                {

                    TutorialSubjectTutorUser.DateModified = DateTime.UtcNow;
                    TutorialSubjectTutorUser.DateCreated = DateTime.UtcNow;
                    TutorialSubjectTutorUser.CreatedUserId = User.Identity.GetUserId();
                    TutorialSubjectTutorUser.ModifiedUserId = User.Identity.GetUserId();

                    await db.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    throw ex;
                }


                return Created(TutorialSubjectTutorUser);
            }


            public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<TutorialSubjectTutorUser> patch)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var entity = await db.TutorialSubjectTutorUser.FindAsync(key);
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
                    if (!db.TutorialSubjectTutorUser.Any(p => p.Id == key))
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
                TutorialSubjectTutorUser TutorialSubjectTutorUser = await db.TutorialSubjectTutorUser.FindAsync(key);
                if (TutorialSubjectTutorUser == null)
                {
                    return NotFound();
                }

                db.TutorialSubjectTutorUser.Remove(TutorialSubjectTutorUser);
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
