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

            return View(user);
        }

        //public async Task<ActionResult> ImportUsers()
        //{

        //    var userJson = System.IO.File.ReadAllText(@"c:\temp\users.json");
        //    var users = JsonConvert.DeserializeObject<dynamic>(userJson);

        //    StringBuilder sbCsv = new StringBuilder();

        //    foreach (var user in users)
        //    {

        //        string[] name = ((string)user.Name).Split(" ".ToCharArray());
        //        string role = user.Level;
        //        string email = user.Email;

 
        //        var dbUser = db.Users.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());

        //        if (dbUser == null)
        //        {

        //            var newUserId = db.Users.Max(x => x.ScanId);

           

        //            dbUser = new User();
        //            dbUser.Firstname = name[0];
        //            dbUser.Surname = name.Length > 1 ? name[1] : "";
        //            dbUser.ScanId = newUserId + 1;
        //            dbUser.Role = role;
        //            dbUser.EmailConfirmed = true;
        //            dbUser.AgentId = null;
        //            dbUser.ServiceId = null;
        //            dbUser.Email = user.Email;
        //            dbUser.UserName = user.Email;
        //            dbUser.DateModified = DateTime.UtcNow;
        //            dbUser.DateCreated = DateTime.UtcNow;
        //            dbUser.ModifiedBy = "e76914f4-0583-456a-b27e-9fdad37759bd";
        //            dbUser.CreatedBy = "e76914f4-0583-456a-b27e-9fdad37759bd";

        //            var password = PasswordService.Generate();

        //            sbCsv.AppendLine(string.Format("{0},{1},{2}", name, email, password));

        //            var result = await UserManager.CreateAsync(dbUser, password);

        //            await UserManager.AddToRoleAsync(dbUser.Id, role);

        //            bool dbs = true;

        //        }

        //        Console.WriteLine(name + " " + email);


        //    }

        //    string csvText = sbCsv.ToString();

        //    return new HttpStatusCodeResult(HttpStatusCode.OK);

        //}


        //public async Task<ActionResult> ImportUsers()
        //{

        //    using (var textReader = System.IO.File.OpenText(@"C:\Temp\users.csv"))
        //    {

        //        var csv = new CsvReader(textReader);
        //        var records = csv.GetRecords<ImportUser>();

        //        foreach(var record in records)
        //        {

        //            var user = new User()
        //            {
        //                Firstname = record.Firstname,
        //                Surname = record.Lastname,
        //                Email = record.Email

        //            };


        //            user.EmailConfirmed = true;
        //            user.UserName = user.Email;
        //            user.DateModified = DateTime.UtcNow;
        //            user.DateCreated = DateTime.UtcNow;
        //            user.ModifiedBy = User.Identity.Name;

        //            var result = await UserManager.CreateAsync(user, PasswordService.Generate());


        //            if (result.Succeeded)
        //            {

        //                //Users = u
        //                //Tool Owners = t
        //                //Program Owners = p

        //                var type = record.Type.ToLower();

        //                if (type.Equals("t"))
        //                {
        //                    await UserManager.AddToRoleAsync(user.Id, "Tool Owner");
        //                }
        //                else if (type.Equals("p"))
        //                {
        //                    await UserManager.AddToRoleAsync(user.Id, "Program Owner");

        //                }
        //                else
        //                {
        //                    await UserManager.AddToRoleAsync(user.Id, "User");
        //                }


        //            }

        //                bool b = true;
        //        }

                

        //    }

        //    return new HttpStatusCodeResult(HttpStatusCode.OK);
        //}

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