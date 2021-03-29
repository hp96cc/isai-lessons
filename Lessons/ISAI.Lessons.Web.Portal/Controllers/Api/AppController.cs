using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.EntityFramework.ViewModels;
using ISAI.Lessons.Web.Portal.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers.Api
{

    [Authorize(Roles = "AppUser")]
    public class AppController : BaseApiController
    {

        int _customerId;
        int _appId;

        public AppController()
        {
            _customerId = Convert.ToInt32(User.Identity.GetClaim("CustomerId"));
            _appId = Convert.ToInt32(User.Identity.GetClaim("AppId"));
        }


        [Route("api/app/register")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<RegisterResponseViewModel> Register(RegisterRequestViewModel model)
        {
            var customer = await db.Customer.FirstOrDefaultAsync(x => x.AppId == _appId && x.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));
            var errors = new List<string>();

            if (model.Password != model.PasswordConfirm)
            {
                errors.Add("Passwords do not match");
            }

            if (!PasswordCheckService.IsStrongPassword(model.Password))
            {
                errors.Add("Password is not strong enough. Passwords must be a minimum of 8 characters and contain a lowercase characters, an upper case characters and a number or symbol .");
            }

            if (customer != null)
            {
                errors.Add("User already exists with given email address.");
            }
            else if (errors.Count == 0)
            {
                var email = model.Email.ToLower().Trim();
                var credentials = new Savage.Credentials.Credentials(email, model.Password);
                var saltAndHashedPassword = credentials.CreateSaltAndHashedPassword();

                var salt = saltAndHashedPassword.Salt;
                var hashedPassword = saltAndHashedPassword.HashedPassword;

                var saltBase64 = Convert.ToBase64String(salt);
                var hashedPasswordBase64 = Convert.ToBase64String(hashedPassword);

                customer = new Customer()
                {
                    AppId = _appId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = email,
                    PasswordSalt = saltBase64,
                    PasswordHash = hashedPasswordBase64,
                    AcceptMarketing = model.AcceptMarketing
                };

                db.Customer.Add(customer);
                await db.SaveChangesAsync();

                return new RegisterResponseViewModel()
                {
                    Customer = customer
                };

            }

            return new RegisterResponseViewModel()
            {
                Customer = null,
                Errors = errors
            };

        }

        [Route("api/app/customer")]
        [HttpPost]
        public async Task<Customer> Customer()
        {
            var customer = await db.Customer.FirstOrDefaultAsync(x => 
            x.Id == _customerId && 
            x.AppId == _appId && 
            x.Deleted == false);
            
            return customer;

        }


        [Route("api/app/lesson")]
        [HttpPost]
        public async Task<Lesson> Lesson(int lessonId)
        {
            var lesson = await db.Lesson.FirstOrDefaultAsync(x =>
            x.Id == lessonId &&
            x.Deleted == false);

            return lesson;

        }


        [Route("api/app/subscriptions")]
        [HttpPost]
        public async Task<List<Subscription>> Subscriptions()
        {

            var dateTimeNow = DateTime.UtcNow;

            var subscriptions = await db.Subscription.Where(x => 
            x.CustomerId == _customerId && 
            x.Deleted == false && 
            x.Active == true &&
            x.StartDate <= dateTimeNow &&
            x.EndDate >= dateTimeNow
            ).ToListAsync();

            return subscriptions;
        }

        [Route("api/app/customerdevices")]
        [HttpPost]
        public async Task<List<CustomerDevice>> CustonerDevices()
        {

            var devices = await db.CustomerDevice.Where(x =>
            x.CustomerId == _customerId &&
            x.Deleted == false
            ).ToListAsync();

            return devices;
        }


        [Route("api/app/customeractivity")]
        [HttpPost]
        public async Task<List<CustomerActivity>> CustomerActivity()
        {

            var activity = await db.CustomerActivity
                .Include(x => x.CustomerDevice)
                .Where(x =>
                x.CustomerDevice.CustomerId == _customerId &&
                x.Deleted == false
            ).ToListAsync();

            return activity;
        }


    }
}