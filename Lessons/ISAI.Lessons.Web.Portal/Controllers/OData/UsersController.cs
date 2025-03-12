using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.Web.Portal.App_Start;
using ISAI.Lessons.Web.Portal.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.OData;
using Newtonsoft.Json;
using Scf.Core.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ISAI.Lessons.Web.Portal.Controllers.OData
{

    [Authorize]
    public class UsersController : BaseODataController
    {

        [EnableQueryWithSearch(EnsureStableOrdering = true, ModelType = typeof(User))]
        public IQueryable<User> GetUsers()
        {
            return db.Users.Where(x => x.Deleted == false && x.Email != "support@isai.co.uk");
        }


        [EnableQuery]
        public SingleResult<User> GetUser([FromODataUri] string key)
        {
            return SingleResult.Create(db.Users.Where(x => x.Id == key));
        }

        public async Task<IHttpActionResult> Put([FromODataUri] string key, User patch)
        {
            var t = await Request.Content.ReadAsStringAsync();


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
                if (!db.Users.Any(p => p.Id == key))
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


        public async Task<IHttpActionResult> Post(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                user.Fullname = string.Format("{0} {1}", user.Firstname, user.Surname);
                user.EmailConfirmed = true;
                user.UserName = user.Email;
                user.DateModified = DateTime.UtcNow;
                user.DateCreated = DateTime.UtcNow;
                user.ModifiedBy = User.Identity.Name;

                var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var result = await userManager.CreateAsync(user, PasswordService.Generate());

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user.Id, user.Role);
                    await UpdateUserTutorSubjects(user);
                    return Created(user);

                 
                }
                else
                {

                    throw new Exception(result.Errors.First());
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }



        }

        public async Task<IHttpActionResult> Patch([FromODataUri] string key, Delta<User> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Users.FirstOrDefaultAsync(x => x.Id == key);
            if (entity == null)
            {
                return NotFound();
            }

            patch.Patch(entity);


            //User Manager
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roles = await userManager.GetRolesAsync(entity.Id);
            await userManager.RemoveFromRolesAsync(entity.Id, roles.ToArray());
            await userManager.AddToRoleAsync(entity.Id, entity.Role);

            entity.UserName = entity.Email; //TODO: does this need error handling
            entity.Fullname = string.Format("{0} {1}", entity.Firstname, entity.Surname);
            entity.DateModified = DateTime.UtcNow;
            entity.ModifiedBy = User.Identity.GetUserId();

            try
            {
                await db.SaveChangesAsync();
                await UpdateUserTutorSubjects(entity);

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!db.Users.Any(p => p.Id == key))
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

        async Task UpdateUserTutorSubjects(User user)
        {
            var tutorialSubjectsIds = JsonConvert.DeserializeObject<List<int>>(user.AllowedTutorialSubjectTutorUserJson);
            var tutorialSubjectTutorUsers = await db.TutorialSubjectTutorUser.Where(x => x.Deleted == false && x.TutorUserId == user.Id).ToListAsync();

            foreach (var tutorialSubjectTutorUser in tutorialSubjectTutorUsers)
            {
                if (!tutorialSubjectsIds.Contains(tutorialSubjectTutorUser.TutorialSubjectId))
                {
                    db.TutorialSubjectTutorUser.Remove(tutorialSubjectTutorUser);
                }
            }

            foreach (var tutorialSubjectsId in tutorialSubjectsIds)
            {
                if (!tutorialSubjectTutorUsers.Any(x => x.TutorialSubjectId == tutorialSubjectsId))
                {
                    var serviceUser = new TutorialSubjectTutorUser()
                    {
                        TutorialSubjectId = tutorialSubjectsId,
                        TutorUserId = user.Id,
                        CreatedUserId = User.Identity.GetUserId(),
                        ModifiedUserId = User.Identity.GetUserId(),
                        DateCreated = DateTime.UtcNow,
                        DateModified = DateTime.UtcNow

                    };

                    db.TutorialSubjectTutorUser.Add(serviceUser);
                }
            }

            await db.SaveChangesAsync();

        }



        public async Task<IHttpActionResult> Delete([FromODataUri] string key)
        {
            User user = db.Users.Find(key);
            if (user == null)
            {
                return NotFound();
            }

            //Clear out email and username so email can be re-used
            user.Email = "deleted_" + Guid.NewGuid().ToString() + "@deleted.com";
            user.UserName = user.Email;
            user.Deleted = true;

            user.DateModified = DateTime.UtcNow;
            user.ModifiedBy = User.Identity.Name;

            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roles = await userManager.GetRolesAsync(user.Id);
            await userManager.RemoveFromRolesAsync(user.Id, roles.ToArray());


            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

    }

}
