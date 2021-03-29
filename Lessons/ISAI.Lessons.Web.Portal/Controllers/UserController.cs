using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using ISAI.Lessons.Web.Portal.Helpers;
using Z.EntityFramework.Plus;
using ISAI.Lessons.EntityFramework.Models;
using Scf.Core.Services;

namespace ISAI.Lessons.Web.Portal.Controllers
{
    [Authorize]
    public class UserController : BaseAuthroizationController
    {

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.ItemName = "User";

        }

        // GET: /Subjects/
        public async Task<ActionResult> Index()
        {

            if (User.Identity.GetClaim("Email").Equals("support@isai.co.uk")) {

                return View(await db.Users
                    .Where(x => x.Deleted == false).ToListAsync());
            }

            return View(await db.Users
                .Where(x => x.Deleted == false && x.Email != "support@isai.co.uk").ToListAsync());

        }


        public async Task<ActionResult> Create()
        {
            await LoadRoles();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                user.EmailConfirmed = true;
                user.UserName = user.Email;
                user.DateModified = DateTime.UtcNow;
                user.DateCreated = DateTime.UtcNow;
                user.ModifiedBy = User.Identity.Name;

                var result = await UserManager.CreateAsync(user, PasswordService.Generate());

                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, user.Role);
                    return RedirectToAction("Index");
                }
                else
                {

                    AddErrors(result);
                }


            }

            await LoadRoles();
            return View(user);
        }


        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.FirstOrDefault(x => x.Id == id);


            if (user == null)
            {
                return HttpNotFound();
            }

            await LoadRoles();
            return View(user);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(User user)
        {
            if (ModelState.IsValid)
            {
                var dbUser = db.Users.FirstOrDefault(x => x.Id == user.Id);

                if (TryUpdateModel(dbUser))
                {

                    var roles = await UserManager.GetRolesAsync(user.Id);
                    await UserManager.RemoveFromRolesAsync(user.Id, roles.ToArray());

                    await UserManager.AddToRoleAsync(user.Id, user.Role);

                    dbUser.Fullname = string.Format("{0} {1}", dbUser.Firstname, dbUser.Surname);

                    dbUser.UserName = user.Email;
                    dbUser.DateModified = DateTime.UtcNow;
                    dbUser.ModifiedBy = User.Identity.Name;

                    db.Entry(dbUser).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");

                }
            }

            await LoadRoles();
            return View(user);


        }

        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);

            //Clear out email and username so email can be re-used
            user.Email = "deleted_" + Guid.NewGuid().ToString() + "@deleted.com";
            user.UserName = user.Email;
            user.Deleted = true;

            user.DateModified = DateTime.UtcNow;
            user.ModifiedBy = User.Identity.Name;

            var roles = await UserManager.GetRolesAsync(user.Id);
            await UserManager.RemoveFromRolesAsync(user.Id, roles.ToArray());

            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


    }

}