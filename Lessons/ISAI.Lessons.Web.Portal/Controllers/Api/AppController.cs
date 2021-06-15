using Effortless.Net.Encryption;
using ISAI.Lessons.Core.Services;
using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.EntityFramework.ViewModels;
using ISAI.Lessons.EntityFramework.ViewModels.Stripe;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.ViewModels;
using ISAI.Lessons.Web.Portal.Helpers;
using Microsoft.Azure.Management.Media.Models;
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
using System.Web;
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
        string _base64Key = "Shnlc8favzpU3dBgpUuZ4yktIWPyB/xU3FYcEAWVAVE=";
        string _base64Iv = "GOcuDFIgvv+Sss54DFVjLN/2GFyCP1TZc9HDNKf/cxY=";

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
        public async Task<Lessons.Models.Models.ResponseData<Customer>> Customer()
        {

            var response = new Lessons.Models.Models.ResponseData<Customer>();

            var customer = await db.Customer.FirstOrDefaultAsync(x =>
               x.Id == _customerId &&
               x.AppId == _appId &&
               x.Deleted == false);

            response.Content = customer;
            response.Status = ResponseStatus.OK;

            return response;

        }

        [Route("api/app/savecustomer")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<Customer>> SaveCustomer(Customer customer)
        {

            var response = new Lessons.Models.Models.ResponseData<Customer>();

            if (_customerId > 0)
            {
                var dbCustomer = await db.Customer.FirstOrDefaultAsync(x => x.Id == _customerId);

                if(dbCustomer.Email.Trim().ToLower() != customer.Email.Trim().ToLower())
                {
                    var existingCustomer = await db.Customer.Where(x => dbCustomer.Id != x.Id && x.Email.Trim().ToLower() == customer.Email.Trim().ToLower()).FirstOrDefaultAsync();

                    if(existingCustomer != null)
                    {

                        response.Status = ResponseStatus.UserAlreadyExists;
                        response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                        {
                            new Lessons.Models.Models.ErrorResponse()
                            {
                                Message = "This email cannot be used as it is already in use."
                            }
                        };

                        return response;
                    }
                }

                dbCustomer.FirstName = customer.FirstName;
                dbCustomer.LastName = customer.LastName;
                dbCustomer.Email = customer.Email;
                dbCustomer.Telephone = customer.Telephone;
                dbCustomer.CompanyName = customer.CompanyName;

                dbCustomer.DateModified = DateTime.UtcNow;

                db.Entry(dbCustomer).State = EntityState.Modified;
                await db.SaveChangesAsync();

                response.Content = customer;
                response.Status = ResponseStatus.OK;

                return response;

            }

            throw new HttpResponseException(HttpStatusCode.NotFound);

        }


        [Route("api/app/lesson")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<Lesson>> Lesson(LessonRequestViewModel lessonRequestViewModel)
        {
            var response = new Lessons.Models.Models.ResponseData<Lesson>();

            var lesson = await db.Lesson.FirstOrDefaultAsync(x =>
            x.Id == lessonRequestViewModel.LessonId &&
            x.Deleted == false);

            response.Content = lesson;
            response.Status = ResponseStatus.OK;

            return response;

        }



        [Route("api/app/lessonmediaurl")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<LessonStreamingResponse>> LessonMediaUrl(LessonRequestViewModel lessonRequestViewModel)
        {

            var response = new Lessons.Models.Models.ResponseData<LessonStreamingResponse>();

            var licenceResponse = await CheckSubscription();

            if (licenceResponse.Status == ResponseStatus.OK)
            {

                var lesson = await db.Lesson.FirstOrDefaultAsync(x =>
                    x.Id == lessonRequestViewModel.LessonId &&
                    x.Deleted == false);

                //If licence is not full access, compare access of lesson
                if (licenceResponse.Content.SubscriptionTypeId > 1 && licenceResponse.Content.SubscriptionTypeId != lesson.SubscriptionTypeId)
                {
                    response.Status = ResponseStatus.InvalidLicence;
                    response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                    {
                        new Lessons.Models.Models.ErrorResponse()
                        {
                            Message = "You do not have a licence to access this content."
                        }
                    };

                    return response;
                }

                var deviceRepsonse = await CheckCustomerDevice(lessonRequestViewModel.CustomerDevice);

                if(deviceRepsonse.Status == ResponseStatus.OK)
                {
                    response.Status = deviceRepsonse.Status;
                    response.ErrorResponse = deviceRepsonse.ErrorResponse;
                    response.Content = null;
                }

                if (lessonRequestViewModel.RemoteMediaType == RemoteMediaType.EncryptedStream)
                {
                    var azureMediaService = new AzureMediaService();
                    var urlTuple = await azureMediaService.GetEncryptedStreamingUrlsAsync(null, null, null, string.Format("aes-streaming-locator-{0}", lesson.Id));

                    response.Status = ResponseStatus.OK;
                    response.Content = new LessonStreamingResponse()
                    {
                        StreamingUrl = urlTuple.Item1,
                        Token = urlTuple.Item2
                    };

                }
                else if (lessonRequestViewModel.RemoteMediaType == RemoteMediaType.StandardStream)
                {

                    var azureMediaService = new AzureMediaService();
                    var urls = await azureMediaService.GetStreamingUrlsAsync(null, null, null, string.Format("streaming-locator-{0}", lesson.Id), StreamingPolicyStreamingProtocol.Hls);

                    response.Status = ResponseStatus.OK;
                    response.Content = new LessonStreamingResponse()
                    {
                        StreamingUrl = urls[0]
                    };

                    return response;

                }
                else if (lessonRequestViewModel.RemoteMediaType == RemoteMediaType.Download)
                {

                    var azureMediaService = new AzureMediaService();
                    var urls = await azureMediaService.GetStreamingUrlsAsync(null, null, null, string.Format("download-locator-{0}", lesson.Id), StreamingPolicyStreamingProtocol.Download);


                    response.Status = ResponseStatus.OK;
                    response.Content = new LessonStreamingResponse()
                    {
                        StreamingUrl = urls.First(x => x.Contains(".mp4")) //TODO: this needs to be more specific 
                    };

                    return response;
                }
                else
                {
                    response.Status = ResponseStatus.Failed;
                    response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                    {
                        new Lessons.Models.Models.ErrorResponse()
                        {
                            Message = "Invalid Remote Media Type."
                        }
                    };

                    return response;

                }


            } else
            {

                response.Status = licenceResponse.Status;
                response.ErrorResponse = licenceResponse.ErrorResponse;
                response.Content = null;


            }

            return response;

        }

        async Task<Lessons.Models.Models.ResponseBase> CheckCustomerDevice(CustomerDeviceViewModel customerDevice)
        {

            var response = new Lessons.Models.Models.ResponseBase();

            var customer = await db.Customer.FirstAsync(x => x.Id == _customerId);
            var customerDevices = await db.CustomerDevice.Where(x => x.CustomerId == _customerId && x.Deleted == false).ToListAsync();

            var dbCustomerDevice = customerDevices.FirstOrDefault(x => x.DeviceIdentifier == customerDevice.DeviceIdentifier);

            if(dbCustomerDevice != null)
            {
                //Device exists, allow
                response.Status = ResponseStatus.OK;
                return response;
            }

            if(customerDevices.Count >= customer.MaxDevicesAllowed)
            {
                //Maximum devices already allowed
                response.Status = ResponseStatus.TooManyDevices;
                response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                {
                    new Lessons.Models.Models.ErrorResponse()
                    {
                        Message = string.Format("Your account already has {0} devices allocated to it. Please remove a device in your account settings to use this one.", customer.MaxDevicesAllowed)
                    }
                };
                return response;

            }

            //Add device
            db.CustomerDevice.Add(new CustomerDevice()
            {
                Name = customerDevice.Name,
                DeviceIdentifier = customerDevice.DeviceIdentifier,
                DeviceType = customerDevice.DeviceType
            });

            await db.SaveChangesAsync();

            response.Status = ResponseStatus.OK;
            return response;
        }

        async Task<Lessons.Models.Models.ResponseData<Subscription>> CheckSubscription()
        {

            var response = new Lessons.Models.Models.ResponseData<Subscription>();

            //Check subscription
            var subscription = await db.Subscription
                .OrderByDescending(x => x.EndDate)
                .FirstOrDefaultAsync(x => x.CustomerId == _customerId);

            if (subscription == null)
            {
                response.Status = ResponseStatus.InvalidLicence;
                response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                {
                    new Lessons.Models.Models.ErrorResponse()
                    {
                        Message = "A valid licence does not exist. Please contact support."
                    }
                };

                return response;

            }
            else if (subscription.EndDate < DateTime.Today)
            {
                response.Status = ResponseStatus.LicenceExpired;
                response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                {
                    new Lessons.Models.Models.ErrorResponse()
                    {
                        Message = "Your licence has expired or has been cancelled. Please contact support."
                    }
                };

                return response;

            }

            response.Content = subscription;
            response.Status = ResponseStatus.OK;
            return response;

        }



        

      

        [AllowAnonymous] 
        [Route("api/app/sendemail")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<bool>> SendEmail(SendEmailRequestViewModel request)
        {
            var response = new Lessons.Models.Models.ResponseData<bool>();
            var graphApi = new MicrosoftGraphApiService();

            try
            {
                await graphApi.SendEmail("noreply@scottishonlinelessons.com", request.Subject, request.Message, new List<string>() { "info@scottishonlinelessons.com" }, null, new List<string>() { "sysadmin@isai.co.uk" }, true);
                response.Content = true;
                response.Status = ResponseStatus.OK;
              

            } catch(Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ISAI.Lessons.Models.Models.ErrorResponse>() { new ISAI.Lessons.Models.Models.ErrorResponse () {
                        Message = ex.Message,
                        ErrorDescription = ex.StackTrace
                    }
                };
                return response;
            }

      
            return response;

        }

        [Route("api/app/lessons")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<List<Lesson>>> Lessons()
        {
            var response = new Lessons.Models.Models.ResponseData<List<Lesson>>();

            var licenceResponse = await CheckSubscription();

            if (licenceResponse.Status == ResponseStatus.OK)
            {

                if (licenceResponse.Content.SubscriptionTypeId == 1)
                {
                    var lessons = await db.Lesson.Where(x => x.Deleted == false && x.AssetId != null).ToListAsync();

                    response.Content = lessons;
                    response.Status = ResponseStatus.OK;
                    return response;
                }
                else
                {
                    var lessons = await db.Lesson.Where(x => x.Deleted == false && x.AssetId != null && x.SubscriptionTypeId == x.SubscriptionTypeId).ToListAsync();

                    response.Content = lessons;
                    response.Status = ResponseStatus.OK;
                    return response;
                }

            } else
            {
                response.Status = licenceResponse.Status;
                response.ErrorResponse = licenceResponse.ErrorResponse;
                return response;
            }

        }

        [AllowAnonymous]
        [Route("api/app/lessongroups")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<List<LessonGroup>>> LessonGroups()
        {

            var response = new Lessons.Models.Models.ResponseData<List<LessonGroup>>();

            var licenceResponse = await CheckSubscription();

            if (licenceResponse.Status == ResponseStatus.OK)
            {

                if (licenceResponse.Content.SubscriptionTypeId == 1)
                {
                    var lessonGroups = await db.LessonGroup.Where(x => x.Deleted == false).ToListAsync();
                    response.Content = lessonGroups;
                    response.Status = ResponseStatus.OK;
                    return response;
                }
                else
                {
                    var lessonGroups = await db.LessonGroup.Where(x => x.Deleted == false && x.SubscriptionTypeId == licenceResponse.Content.SubscriptionTypeId).ToListAsync();
                    response.Content = lessonGroups;
                    response.Status = ResponseStatus.OK;
                    return response;
                }

            }
            else
            {
                response.Status = licenceResponse.Status;
                response.ErrorResponse = licenceResponse.ErrorResponse;
                return response;
            }

  
        }



        [Route("api/app/subscriptions")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<List<Subscription>>> Subscriptions()
        {

            var response = new Lessons.Models.Models.ResponseData<List<Subscription>>();

            var dateTimeNow = DateTime.UtcNow;

            var subscriptions = await db.Subscription.Where(x =>
            x.CustomerId == _customerId &&
            x.Deleted == false &&
            x.Active == true &&
            x.StartDate <= dateTimeNow &&
            x.EndDate >= dateTimeNow
            ).ToListAsync();

            response.Content = subscriptions;
            response.Status = ResponseStatus.OK;

            return response;

        }


        [Route("api/app/cancelsubscriptions")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseBase> CancelSubscriptions()
        {

            var response = new Lessons.Models.Models.ResponseBase();

            var subscriptions = await Subscriptions();

            foreach(var subscription in subscriptions.Content)
            {
                subscription.Active = false;
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;
                db.Entry(subscription).State = EntityState.Modified;
            }

            await db.SaveChangesAsync();

            response.Status = ResponseStatus.OK;
            return response;

        }

        [Route("api/app/restoresubscriptions")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<List<Subscription>>> RestoreSubscriptions()
        {
            var response = new Lessons.Models.Models.ResponseData<List<Subscription>>();

            var subscriptions = await Subscriptions();

            foreach (var subscription in subscriptions.Content)
            {
                subscription.Active = true;
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;
                db.Entry(subscription).State = EntityState.Modified;
            }

            await db.SaveChangesAsync();

            response.Content = subscriptions.Content;
            response.Status = ResponseStatus.OK;
            return response;

        }



        [Route("api/app/customerdevices")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<List<CustomerDevice>>> CustomerDevices()
        {

            var response = new Lessons.Models.Models.ResponseData<List<CustomerDevice>>();

            var devices = await db.CustomerDevice.Where(x =>
            x.CustomerId == _customerId &&
            x.Deleted == false
            ).ToListAsync();

            response.Content = devices;
            response.Status = ResponseStatus.OK;

            return response;
        }


        [Route("api/app/customeractivity")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseData<List<CustomerActivity>>> CustomerActivity()
        {

            var response = new Lessons.Models.Models.ResponseData<List<CustomerActivity>>();


            var activity = await db.CustomerActivity
                .Include(x => x.CustomerDevice)
                .Where(x =>
                x.CustomerDevice.CustomerId == _customerId &&
                x.Deleted == false
            ).ToListAsync();

            response.Content = activity;
            response.Status = ResponseStatus.OK;

            return response;
        }

        [AllowAnonymous]
        [Route("api/app/sendresetpasswordemail")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseBase> SendResetPasswordEmail(SendResetPasswordEmailViewModel model)
        {

            var response = new Lessons.Models.Models.ResponseBase();
            var customer = await db.Customer.FirstOrDefaultAsync(x => x.AppId == model.AppId && x.Email.Trim().ToLower() == model.Email.Trim().ToLower());

            if(customer != null)
            {
                var digest = string.Format("{0}|{1}|{2}", customer.Id, customer.AppId, DateTime.UtcNow.AddMinutes(60).Ticks);

                byte[] key = Convert.FromBase64String(_base64Key);
                byte[] iv = Convert.FromBase64String(_base64Iv);
                string encryptedDigest = Strings.Encrypt(digest, key, iv);

                var passwordResetLink = "https://scottishonlinelessons.com/reset-password?digest=" + HttpUtility.UrlEncode(encryptedDigest);

                var templateHtml = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/Email Templates/ResetPasswordEmailTemplate.html"));
                templateHtml = templateHtml.Replace("{{name}}", string.Format("{0} {1}", customer.FirstName, customer.LastName));
                templateHtml = templateHtml.Replace("{{link}}", passwordResetLink);

                MicrosoftGraphApiService graphService = new MicrosoftGraphApiService();
                await graphService.SendEmail(
                        "noreply@scottishonlinelessons.com",
                        "Reset Password - Scottish Online Lessons",
                        templateHtml,
                        new List<string>() { customer.Email },
                        null,
                        new List<string>() { "sysadmin@isai.co.uk" },
                        true);

            }

            response.Status = ResponseStatus.OK;
            return response;

        }

        [AllowAnonymous]
        [Route("api/app/updatepassword")]
        [HttpPost]
        public async Task<Lessons.Models.Models.ResponseBase> UpdatePassword(UpdatePasswordViewModel model)
        {

            var response = new Lessons.Models.Models.ResponseBase();

            byte[] key = Convert.FromBase64String(_base64Key);
            byte[] iv = Convert.FromBase64String(_base64Iv);
            var digest = Strings.Decrypt(model.Digest, key, iv).Split("|".ToCharArray());
            var customerId = Convert.ToInt32(digest[0]);
            var appId = Convert.ToInt32(digest[1]);
            var expiry = new DateTime(Convert.ToInt64(digest[2]), DateTimeKind.Utc);

            if(DateTime.UtcNow > expiry)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                {
                    new Lessons.Models.Models.ErrorResponse()
                    {
                        Message = "Email password link has expired"
                    }
                };
                return response;
            }

            var customer = await db.Customer.FirstOrDefaultAsync(x => x.AppId == appId && x.Id == customerId);

            if(customer == null)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                {
                    new Lessons.Models.Models.ErrorResponse()
                    {
                        Message = "Customer does not exist"
                    }
                };
                return response;
            }


            if (model.Password != model.ConfirmPassword)
            {
              
               response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                {
                    new Lessons.Models.Models.ErrorResponse()
                    {
                        Message = "Passwords do not match"
                    }
                };
                return response;
            }

            if (!PasswordCheckService.IsStrongPassword(model.Password))
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>()
                {
                    new Lessons.Models.Models.ErrorResponse()
                    {
                        Message = "Password is not strong enough. Passwords must be a minimum of 8 characters and contain a lowercase characters, an upper case characters and a number or symbol ."
                    }
                };
                return response;

            }

            var email = customer.Email.ToLower().Trim();
            var credentials = new Savage.Credentials.Credentials(email, model.Password);
            var saltAndHashedPassword = credentials.CreateSaltAndHashedPassword();

            var salt = saltAndHashedPassword.Salt;
            var hashedPassword = saltAndHashedPassword.HashedPassword;

            var saltBase64 = Convert.ToBase64String(salt);
            var hashedPasswordBase64 = Convert.ToBase64String(hashedPassword);

            customer.PasswordSalt = saltBase64;
            customer.PasswordHash = hashedPasswordBase64;
            customer.DateModified = DateTime.UtcNow;

            db.Entry(customer).State = EntityState.Modified;

            await db.SaveChangesAsync();

            response.Status = ResponseStatus.OK;
            return response;

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

        async Task<RegisterResponseViewModel> RegisterCustomer(RegisterRequestViewModel model)
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
                    customer.MaxDevicesAllowed = 2;

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
                        HasCompletedCheckout = false,
                        MaxDevicesAllowed = 2
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


        [Route("api/app/signupaccesscode")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<Lessons.Models.Models.ResponseData<Customer>> SignupAccessCode(RegisterRequestViewModel request)
        {
            var response = new Lessons.Models.Models.ResponseData<Customer>();

            try
            {

                var accessCode = request.AccessCode.Trim().ToUpper();
                var subscriptionCode = await db.SubscriptionCode.FirstOrDefaultAsync(x =>
                    x.Code == accessCode &&
                    x.ValidFrom >= DateTime.UtcNow &&
                    x.UsedDateTime.HasValue == false);

                if (subscriptionCode != null)
                {

                    var register = await RegisterCustomer(request);

                    if (register.Customer == null)
                    {
                        if(register.Errors != null && register.Errors.Count > 0)
                        {
                            throw new Exception(register.Errors[0]);
                        }
                        else
                        {
                            throw new Exception("Could not create customer");
                        }
                       
                    }

                    await CreateCustomerSubscription((Customer)register.Customer, null, subscriptionCode);

                    response.Content = (Customer)register.Customer;
                    response.Status = ResponseStatus.OK;

                    return response;

                }
                else
                {
                    throw new Exception("Code not valid");
                }

            } catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<Lessons.Models.Models.ErrorResponse>() { new Lessons.Models.Models.ErrorResponse () {
                        Message = ex.Message,
                        ErrorDescription = ex.StackTrace
                    }
                };
                return response;
            }


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

        async Task CreateCustomerSubscription(Customer customer, Session session, SubscriptionCode subscriptionCode = null)
        {

            if(subscriptionCode != null)
            {


                var subscription = new Subscription()
                {
                    SubscriptionTypeId = subscriptionCode.SubscriptionTypeId,
                    Name = "Access Code Subscription",
                    CustomerId = customer.Id,
                    StripeSubscriptionId = subscriptionCode.Code,
                    Active = true,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(subscriptionCode.LicenceDays), //TODO: need to support other
                    DateModified = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow,
                    CreatedUserId = _adminUserId,
                    ModifiedUserId = _adminUserId
                };
                db.Subscription.Add(subscription);

                await db.SaveChangesAsync();

                subscriptionCode.UsedDateTime = DateTime.UtcNow;
                subscriptionCode.SubscriptionId = subscription.Id;
                subscriptionCode.DateModified = DateTime.UtcNow;
                subscriptionCode.ModifiedUserId = _adminUserId;

                customer.StripeCustomerId = subscriptionCode.Code;
                customer.HasCompletedCheckout = true;
                customer.DateModified = DateTime.UtcNow;
                customer.ModifiedUserId = _adminUserId;

                db.Entry(customer).State = EntityState.Modified;

                await db.SaveChangesAsync();
            } 
            else
            {

                var subscription = await db.Subscription.FirstOrDefaultAsync(x => x.CustomerId == customer.Id && x.Deleted == false);

                string subscriptionName;
                int subscriptionTypeId;
                int subscriptionMonths;

                switch (session.LineItems.ElementAt(0).Price.Id)
                {

                    case "price_1J0LLgJ81SbG6nzad3BrKr0L":
                        subscriptionName = "Secondary Annual Subscription";
                        subscriptionTypeId = 2;
                        subscriptionMonths = 12;
                        break;

                    case "price_1J0LLMJ81SbG6nzasmeqA5KF":
                        subscriptionName = "Secondary Monthly Subscription";
                        subscriptionTypeId = 2;
                        subscriptionMonths = 1;
                        break;

                    case "price_1IxXUFJ81SbG6nzav7NG3Vto":
                        subscriptionName = "Primary Monthly Subscription";
                        subscriptionTypeId = 1;
                        subscriptionMonths = 1;
                        break;

                    case "price_1IxXTvJ81SbG6nzajuriP6XZ":
                    default:
                        subscriptionName = "Primary Annual Subscription";
                        subscriptionTypeId = 1;
                        subscriptionMonths = 12;
                        break;
                }

                if (subscription != null)
                {
                    subscription.Name = subscriptionName;
                    subscription.Active = true;
                    subscription.SubscriptionTypeId = subscriptionTypeId;
                    subscription.EndDate = subscription.EndDate.AddMonths(subscriptionMonths);
                    subscription.DateModified = DateTime.UtcNow;
                    subscription.ModifiedUserId = _adminUserId;

                    db.Entry(subscription).State = EntityState.Modified;

                }
                else
                {
                    subscription = new Subscription()
                    {
                        Name = subscriptionName,
                        CustomerId = customer.Id,
                        StripeSubscriptionId = session.SubscriptionId,
                        SubscriptionTypeId = subscriptionTypeId,
                        Active = true,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(subscriptionMonths),
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


        #region Legacy App Methods

       

        [AllowAnonymous] //remove at runtime
        [Route("api/app/downloadscreenshots")]
        [HttpPost]
        public async Task DownloadScreenShots()
        {

            var lessons = await db.Lesson.Where(x =>
            x.AssetId != null &&
            x.Deleted == false).ToListAsync();


            foreach (var lesson in lessons)
            {

                try
                {

                    var azureMediaService = new AzureMediaService();
                    var urls = await azureMediaService.GetStreamingUrlsAsync(null, null, null, "download-locator-" + lesson.Id.ToString(), StreamingPolicyStreamingProtocol.Download);

                    var url = urls.First(x => x.Contains(".jpg"));

                    var saveFilePath = @"C:\Temp\thumbs\thumb_" + lesson.Id + ".jpg";

                    HttpClient client = new HttpClient();
                    var response = await client.GetAsync(url);
                    using (var fs = new FileStream(
                        saveFilePath,
                        FileMode.CreateNew))
                    {
                        await response.Content.CopyToAsync(fs);
                    }

                }
                catch (Exception ex)
                {

                }

            }

        }


        #endregion

    }


    #endregion

}