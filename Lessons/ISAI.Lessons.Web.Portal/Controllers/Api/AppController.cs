using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.EntityFramework.ViewModels;
using ISAI.Lessons.EntityFramework.ViewModels.Stripe;
using ISAI.Lessons.Web.Portal.Helpers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Customer = ISAI.Lessons.EntityFramework.Models.Customer;
using Subscription = ISAI.Lessons.EntityFramework.Models.Subscription;

namespace ISAI.Lessons.Web.Portal.Controllers.Api
{

    [Authorize(Roles = "AppUser")]
    public class AppController : BaseApiController
    {

        int _customerId;
        int _appId;

        StripeOptions options;
        IStripeClient client;

        string _adminUserId = "efef3c10-4e2c-4aca-baf7-7477377d1d73";

        public AppController()
        {
            if (User.Identity.IsAuthenticated)
            {
                _customerId = Convert.ToInt32(User.Identity.GetClaim("CustomerId"));
                _appId = Convert.ToInt32(User.Identity.GetClaim("AppId"));
            }

            this.options = new StripeOptions()
            {
                PublishableKey = ConfigurationManager.AppSettings["STRIPE_PUBLISHABLE_KEY"],
                SecretKey = ConfigurationManager.AppSettings["STRIPE_SECRET_KEY"],
                WebhookSecret = ConfigurationManager.AppSettings["STRIPE_WEBHOOK_SECRET"],
                BasicPrice = ConfigurationManager.AppSettings["BASIC_PRICE_ID"], //TODO: remove?
                ProPrice = ConfigurationManager.AppSettings["PRO_PRICE_ID"], //TODO: remove?
                Domain = ConfigurationManager.AppSettings["DOMAIN"], //TODO: remove?
            };


            this.client = new StripeClient(this.options.SecretKey);
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
        public async Task<Lesson> Lesson(LessonRequestViewModel lessonRequestViewModel)
        {
            var lesson = await db.Lesson.FirstOrDefaultAsync(x =>
            x.Id == lessonRequestViewModel.LessonId &&
            x.Deleted == false);

            return lesson;

        }

        [AllowAnonymous]
        [Route("api/app/lessons")]
        [HttpPost]
        public async Task<List<Lesson>> Lessons()
        {
            var lesson = await db.Lesson.Where(x => x.Deleted == false).ToListAsync();
            return lesson;

        }

        [AllowAnonymous]
        [Route("api/app/lessongroups")]
        [HttpPost]
        public async Task<List<LessonGroup>> LessonGroups()
        {
            var lessonGroups = await db.LessonGroup.Where(x => x.Deleted == false).ToListAsync();
            return lessonGroups;

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


        [Route("api/app/cancelsubscriptions")]
        [HttpPost]
        public async Task<List<Subscription>> CancelSubscriptions()
        {

            var subscriptions = await Subscriptions();

            foreach(var subscription in subscriptions)
            {
                subscription.Active = false;
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;
                db.Entry(subscription).State = EntityState.Modified;
            }

            await db.SaveChangesAsync();

            return subscriptions;

        }

        [Route("api/app/restoresubscriptions")]
        [HttpPost]
        public async Task<List<Subscription>> RestoreSubscriptions()
        {

            var subscriptions = await Subscriptions();

            foreach (var subscription in subscriptions)
            {
                subscription.Active = true;
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;
                db.Entry(subscription).State = EntityState.Modified;
            }

            await db.SaveChangesAsync();

            return subscriptions;

        }



        [Route("api/app/customerdevices")]
        [HttpPost]
        public async Task<List<CustomerDevice>> CustomerDevices()
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

        [Route("api/app/forgotpassword")]
        [HttpPost]
        public async Task ForgotPassword()
        {
            var customer = await Customer();

            //TODO: send password email

        }


        #region Stripe Integration

        [Route("api/app/stripecreatepayemntsession")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<CreateCustomerPayemntSessionResponse> CreateCustomerPayemntSession(CreateCustomerPayemntSessionRequest request)
        {

            try
            {

                var register = await RegisterCustomer(request);

            if (register.Customer == null)
            {
                return new CreateCustomerPayemntSessionResponse()
                {
                    SessionId = null,
                    Errors = register.Errors
                };
            }


            var options = new SessionCreateOptions
            {
                // See https://stripe.com/docs/api/checkout/sessions/create
                //SuccessUrl = "https://example.com/success.html?session_id={CHECKOUT_SESSION_ID}",
                //CancelUrl = "https://example.com/canceled.html",
                ClientReferenceId = register.Customer.Id.ToString(),
                SuccessUrl = request.SuccessUrl + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = request.CancelUrl + "?session_id={CHECKOUT_SESSION_ID}",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = request.PriceId,
                        Quantity = 1,
                    },
                },
            };
            var service = new SessionService(this.client);
           
                var session = await service.CreateAsync(options);

                register.Customer.PaymentSessionId = session.Id;
                db.Entry(register.Customer).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new CreateCustomerPayemntSessionResponse
                {
                    SessionId = session.Id,
                };
        }
            catch (StripeException e)
            {
                Console.WriteLine(e.StripeError.Message);

                var errorMessage = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = e.StripeError.Message
                };

                throw new HttpResponseException(errorMessage);


    }
            catch (Exception ex)
            {

                
                var errorMessage = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = ex.Message
                };

                throw new HttpResponseException(errorMessage);


}

        }

        public async Task<RegisterResponseViewModel> RegisterCustomer(RegisterRequestViewModel model)
        {
            var customer = await db.Customer.FirstOrDefaultAsync(x => x.AppId == model.AppId && x.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));
            var errors = new List<string>();

            if (model.Password != model.PasswordConfirm)
            {
                errors.Add("Passwords do not match");
            }

            if (!PasswordCheckService.IsStrongPassword(model.Password))
            {
                errors.Add("Password is not strong enough. Passwords must be a minimum of 8 characters and contain a lowercase characters, an upper case characters and a number or symbol .");
            }


            if (customer != null && customer.HasCompletedCheckout)
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

                if (customer != null)
                {
                    //Customer has not completed checkout so we will override them here
                    customer.AppId = model.AppId;
                    customer.FirstName = model.FirstName;
                    customer.LastName = model.LastName;
                    customer.Email = email;
                    customer.PasswordSalt = saltBase64;
                    customer.PasswordHash = hashedPasswordBase64;
                    customer.AcceptMarketing = model.AcceptMarketing;
                    customer.HasCompletedCheckout = false;

                    db.Entry(customer).State = EntityState.Modified;
                }
                else
                {
                    customer = new Customer()
                    {
                        AppId = model.AppId,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = email,
                        PasswordSalt = saltBase64,
                        PasswordHash = hashedPasswordBase64,
                        AcceptMarketing = model.AcceptMarketing,
                        HasCompletedCheckout = false
                    };

                    db.Customer.Add(customer);
                }



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

        [Route("api/app/confirmcustomersubscription")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<Customer> ConfirmCustomerSubscription(ConfirmCustomerSubscriptionRequest request)
        {
            var postRegistrationAccessCode = Guid.NewGuid().ToString();
            var customer = await db.Customer.FirstAsync(x => x.PaymentSessionId == request.SessionId && x.AppId == request.AppId);
            customer.PostRegistrationAccessCode = postRegistrationAccessCode;

            var service = new SessionService(this.client);
            var session = await service.GetAsync(request.SessionId);


            await CreateCustomerSubscription(customer, session);

            return customer;

        }




        [Route("api/app/stripecustomerportal")]
        [HttpPost]
        public async Task<Stripe.BillingPortal.Session> CustomerPortal(StripeCustomerPortalRequest request)
        {

            try
            {

                var customer = await db.Customer.FirstAsync(x => x.Id == _customerId);

                var options = new Stripe.BillingPortal.SessionCreateOptions
                {
                    Customer = customer.StripeCustomerId,
                    ReturnUrl = request.ReturnUrl,
                };

                var service = new Stripe.BillingPortal.SessionService(this.client);
                var session = await service.CreateAsync(options);

                return session;

            } catch(Exception ex)
            {
                throw ex;
            }

        }



        [Route("api/app/stripepayemntsession")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<Session> CheckoutSession(string sessionId)
        {
            var service = new SessionService(this.client);
            var session = await service.GetAsync(sessionId);
            return session;
        }

        [Route("api/app/stripewebhook")]
        [HttpPost]
        [AllowAnonymous]
        public async Task StripeWebhook()
        {

            var json = await Request.Content.ReadAsStringAsync();
            var stripeSignature = Request.Headers.First(x => x.Key.Equals("Stripe-Signature")).Value.ElementAt(0);

            Event stripeEvent;


            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    options.WebhookSecret
                );
                Console.WriteLine($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something failed {e}");
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            int customerId;

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":

                    var checkOutComplete = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    customerId = Convert.ToInt32(checkOutComplete.ClientReferenceId);
                    var customer = await db.Customer.FirstAsync(x => x.Id == customerId);
                    customer.StripeCustomerId = checkOutComplete.CustomerId;
                    await CreateCustomerSubscription(customer, checkOutComplete);

                    break;
                case "invoice.paid":
                    // Continue to provision the subscription as payments continue to be made.
                    // Store the status in your database and check when a user accesses your service.
                    // This approach helps you avoid hitting rate limits.
                    var paidInvoice = stripeEvent.Data.Object as Stripe.Invoice;
                    await UpdateCustomerSubscription(paidInvoice.SubscriptionId, true);

                    break;
                case "invoice.payment_failed":
                    // The payment failed or the customer does not have a valid payment method.
                    // The subscription becomes past_due. Notify your customer and send them to the
                    // customer portal to update their payment information.
                    var failedInvoice = stripeEvent.Data.Object as Stripe.Invoice;
                    await UpdateCustomerSubscription(failedInvoice.SubscriptionId, false);

                    break;
                case "customer.subscription.deleted":
                    // The payment failed or the customer does not have a valid payment method.
                    // The subscription becomes past_due. Notify your customer and send them to the
                    // customer portal to update their payment information.
                    var deletedSubscription = stripeEvent.Data.Object as Stripe.Subscription;
                    await UpdateCustomerSubscription(deletedSubscription.Id, false);

                    break;
                default:
                    // Unhandled event type
                    break;
            }

        }

        async Task CreateCustomerSubscription(Customer customer, Session session)
        {
            var subscription = await db.Subscription.FirstOrDefaultAsync(x => x.CustomerId == customer.Id && x.Deleted == false);

            if (subscription != null)
            {
                subscription.Name = "Monthly Subscription";
                subscription.Active = true;
                subscription.EndDate = subscription.EndDate.AddMonths(1);
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;

                db.Entry(subscription).State = EntityState.Modified;

            }
            else
            {
                subscription = new Subscription()
                {
                    Name = "Monthly Subscription",
                    CustomerId = customer.Id,
                    StripeSubscriptionId = session.SubscriptionId,
                    Active = true,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    DateModified = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow,
                    CreatedUserId = _adminUserId,
                    ModifiedUserId = _adminUserId
                };

                db.Subscription.Add(subscription);

            }

            customer.StripeCustomerId = session.CustomerId;
            customer.HasCompletedCheckout = true;
            customer.DateModified = DateTime.UtcNow;
            customer.ModifiedUserId = _adminUserId;

            db.Entry(customer).State = EntityState.Modified;

            await db.SaveChangesAsync();

        }



        async Task UpdateCustomerSubscription(string subscriptionId, bool isActive)
        {

            var subscription = await db.Subscription.FirstAsync(x => x.StripeSubscriptionId == subscriptionId && x.Deleted == false);

            subscription.Active = isActive ? true : false;
            subscription.EndDate = isActive ? subscription.EndDate.AddMonths(1) : DateTime.UtcNow;
            subscription.DateModified = DateTime.UtcNow;
            subscription.ModifiedUserId = _adminUserId;

            db.Entry(subscription).State = EntityState.Modified;

        }

    }


    #endregion

}