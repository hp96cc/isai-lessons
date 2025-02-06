using createsend_dotnet;
using CryptoNet;
using Effortless.Net.Encryption;
using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.EntityFramework.ViewModels;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using ISAI.Lessons.Models.ViewModels;
using ISAI.Lessons.Web.Portal.Helpers;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Configuration;
using Customer = ISAI.Lessons.EntityFramework.Models.Customer;
using ResponseStatus = ISAI.Lessons.Models.Enums.ResponseStatus;
using Subscription = ISAI.Lessons.EntityFramework.Models.Subscription;
using TutorialSubject = ISAI.Lessons.Models.ViewModels.TutorialSubject;
using TutorialSubjectGroup = ISAI.Lessons.Models.ViewModels.TutorialSubjectGroup;
using Tutorial = ISAI.Lessons.EntityFramework.Models.Tutorial;
using System.Text;
using ISAI.Lessons.Core.Services;

namespace ISAI.Lessons.Web.Portal.Controllers.Api
{

    [Authorize(Roles = "AppUser")]
    public class AppController : BaseApiController
    {

        int _customerId;
        int _appId;
        string _baseUrl;
        string _baseReturnUrl;

        StripeOptions options;
        IStripeClient client;
        ICryptoNet cryptoNetKey;

        private readonly string _adminUserId = ConfigurationManager.AppSettings["SystemUserId"];
        private readonly string _base64Key = ConfigurationManager.AppSettings["Video.Base64Key"];
        private readonly string _base64Iv = ConfigurationManager.AppSettings["Video.Base64Iv"];
        private readonly string _aesKeyFile = ConfigurationManager.AppSettings["Video.AesKeyFile"];
        private readonly string _tutorialStripePrefix = ConfigurationManager.AppSettings["Stripe.TutorialPrefix"];
        private readonly string _graphSenderAdminAccount = ConfigurationManager.AppSettings["MicrosoftGraph.SenderEmail"];
        private readonly string _systemTimeZone = ConfigurationManager.AppSettings["SystemTimeZone"];
        private readonly string _videoRootFolder = ConfigurationManager.AppSettings["ISAI.Lessons.VideoRootFolder"];
        private readonly string _videoDownloadFolder = ConfigurationManager.AppSettings["ISAI.Lessons.VideoDownloadFolder"];

        public AppController()
        {
            cryptoNetKey = new CryptoNetAes(new FileInfo(_aesKeyFile));

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
            };

            _baseUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.Portal.Url"];
            _baseReturnUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.ReturnUrl"];

            this.client = new StripeClient(this.options.SecretKey);
        }

        [Route("api/app/customer")]
        [HttpPost]
        public async Task<ResponseData<Customer>> Customer()
        {

            var response = new ResponseData<Customer>();

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
        public async Task<ResponseData<Customer>> SaveCustomer(Customer customer)
        {
            var response = new ResponseData<Customer>();

            if (_customerId > 0)
            {
                var dbCustomer = await db.Customer.FirstOrDefaultAsync(x => x.Id == _customerId);

                if(dbCustomer.Email.Trim().ToLower() != customer.Email.Trim().ToLower())
                {
                    var existingCustomer = await db.Customer.Where(x => dbCustomer.Id != x.Id && x.Email.Trim().ToLower() == customer.Email.Trim().ToLower()).FirstOrDefaultAsync();

                    if(existingCustomer != null)
                    {

                        response.Status = ResponseStatus.UserAlreadyExists;
                        response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
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

        [Route("api/app/tutorialsubjectgroups")]
        [HttpPost]
        public async Task<ResponseData<TutorialSubjectGroupResponseViewModel>> GetTutorialSubjectGroups()
        {
            var response = new ResponseData<TutorialSubjectGroupResponseViewModel>()
            {
                Content = new TutorialSubjectGroupResponseViewModel()
            };

            try
            {
                var tutorialSubjectGroups = await db.TutorialSubjectGroup
                    .Where(x => x.Deleted == false)
                    .Select(x => new TutorialSubjectGroup()
                    {
                        Id = x.Id,
                        Name = x.Name,
                    })
                    .ToListAsync();

                response.Content.TutorialSubjectGroups = tutorialSubjectGroups;
                response.Status = ResponseStatus.OK;
                return response;
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
                            {
                                Message = ex.Message
                        }
                    };

                return response;
            }

        }


        [Route("api/app/tutorial")]
        [HttpPost]
        public async Task<ResponseData<Tutorial>> GetTutorialFromStripeSession(TutorialRequestViewModel request)
        {
            var response = new ResponseData<Tutorial>();

            try
            {

                var service = new SessionService(this.client);
                var session = service.Get(request.StripeSessionId);
                var tutorialId = Convert.ToInt32(session.ClientReferenceId.Replace(_tutorialStripePrefix, string.Empty));

                var tutorial = await db.Tutorial
                    .Include(x => x.TutorUser)
                    .Include(x => x.TutorialSubject)
                    .Include(x => x.TutorialSubject.TutorialSubjectGroup)
                    .Include(x => x.Lesson)
                    .FirstAsync(x => x.Id == tutorialId);

                response.Content = tutorial;
                response.Status = ResponseStatus.OK;
                return response;
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
                            {
                                Message = ex.Message
                        }
                    };

                return response;
            }

        }

        [Route("api/app/tutorials")]
        [HttpPost]
        public async Task<ResponseData<TutorialResponseViewModel>> GetTutorials()
        {
            var response = new ResponseData<TutorialResponseViewModel>()
            {
                Content = new TutorialResponseViewModel()
            };

            try
            {
                var tutorials = await db.Tutorial
                    .Include(x => x.TutorUser)
                    .Where(x => x.CustomerId == _customerId && x.Deleted == false)
                    .OrderByDescending(x => x.DateTimeStart)
                    .ToListAsync();


                response.Content.Tutorials = tutorials.Select(x => new Lessons.Models.ViewModels.Tutorial()
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsCompleted = x.DateTimeStart > DateTime.Now,
                    Date = x.DateTimeStart.Date.ToString("dddd, dd MMMM yyyy"),
                    TimeStart = x.DateTimeStart.ToString("HH:mm"),
                    TimeEnd = x.DateTimeEnd.ToString("HH:mm"),
                    TeamsLink = x.TeamsLink,
                    TutorName = x.TutorUser.Firstname + " " + x.TutorUser.Surname
                }).ToList();
                response.Status = ResponseStatus.OK;
                return response;
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
                            {
                                Message = ex.Message
                        }
                    };

                return response;
            }

        }

        [Route("api/app/tutorialsubjects")]
        [HttpPost]
        public async Task<ResponseData<TutorialSubjectResponseViewModel>> GetTutorialSubjectGroups(TutorialSubjectRequestViewModel request)
        {
            var response = new ResponseData<TutorialSubjectResponseViewModel>()
            {
                Content = new TutorialSubjectResponseViewModel()
            };

            try
            {
                var tutorialSubjects = await db.TutorialSubject
                    .Where(x => x.TutorialSubjectGroupId == request.TutorialGroupId && x.Deleted == false)
                    .Select(x => new TutorialSubject()
                    {
                        Id = x.Id,
                        Name = x.Name,
                    })
                    .ToListAsync();

                response.Content.TutorialSubjects = tutorialSubjects;
                response.Status = ResponseStatus.OK;
                return response;
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
                            {
                                Message = ex.Message
                        }
                    };

                return response;
            }

        }

        [Route("api/app/tutors")]
        [HttpPost]
        public async Task<ResponseData<TutorResponseViewModel>> GetTutors(TutorRequestViewModel request)
        {
            var response = new ResponseData<TutorResponseViewModel>()
            {
                Content = new TutorResponseViewModel()
            };

            try
            {
                if(request.TutorialSubjectId.HasValue)
                {
                    var tutors = await db.TutorialSubjectTutorUser
                        .Include(x => x.TutorUser)
                        .Include(x => x.TutorialSubject)
                        .Where(x => x.TutorUser.Deleted == false && x.TutorialSubjectId == request.TutorialSubjectId)
                        .Select(x => new Tutor()
                        {
                            Id = x.TutorUser.Id,
                            Name = x.TutorUser.Firstname + " " + x.TutorUser.Surname,
                        })
                        .Distinct()
                        .ToListAsync();

                    response.Content.Tutors = tutors;

                } 
                else if(request.LesonId.HasValue)
                {
                    var lesson = await db.Lesson
                        .Include(x => x.LessonGroup)
                        .FirstAsync(x => x.Id == request.LesonId);

                    var tutors = await db.TutorialSubjectTutorUser
                       .Include(x => x.TutorUser)
                       .Include(x => x.TutorialSubject)
                       .Where(x => x.TutorUser.Deleted == false && x.TutorialSubjectId == lesson.LessonGroup.TutorialSubjectId)
                       .Select(x => new Tutor()
                       {
                           Id = x.TutorUser.Id,
                           Name = x.TutorUser.Firstname + " " + x.TutorUser.Surname,
                       })
                       .Distinct()
                       .ToListAsync();

                    response.Content.Tutors = tutors;
                } else
                {
                    throw new Exception("Request must specify either SubjectId or LessonId");
                }

                response.Status = ResponseStatus.OK;
                return response;
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
                            {
                                Message = ex.Message
                        }
                    };

                return response;
            }

        }

        [Route("api/app/tutorialtimeslots")]
        [HttpPost]
        public async Task<ResponseData<TutorTimeSlotResponseViewModel>> GetTutotialTimeSlots(TutorTimeSlotRequestViewModel request)
        {
            var response = new ResponseData<TutorTimeSlotResponseViewModel>()
            {
                Content = new TutorTimeSlotResponseViewModel()
            };

            try
            {
                var app = await db.App.FirstAsync(x => x.Id == _appId);
                decimal costPerTimeSlot;

                switch(request.TutorialDuration)
                {
                    case 20:
                        costPerTimeSlot = app.TutorialCost20Minutes;
                        break;

                    case 40:
                        costPerTimeSlot = app.TutorialCost40Minutes;
                        break;

                    case 60:
                        costPerTimeSlot = app.TutorialCost60Minutes;
                        break;

                    default:
                        throw new Exception("Invalid Tutorial Duration");
                }

                var tutorUser = await db.Users.FirstAsync(x => x.Id == request.TutorId && x.Deleted == false);
                var userList = new List<string>() { tutorUser.Email };
                var startDate = request.TutorialDate.Date;
                var endDate = startDate.AddDays(1).AddSeconds(-1);
                var tutorialDuration = request.TutorialDuration;

                var graphApi = new MicrosoftGraphApiService();
                var availabilityResponse = await graphApi.GetAvailability(
                    _graphSenderAdminAccount, 
                    userList, 
                    startDate, 
                    endDate, 
                    _systemTimeZone, 
                    request.TutorialDuration);

                var scheduleInfo = availabilityResponse.Value[0];

                var availableStartTime = scheduleInfo.WorkingHours.StartTime.Value.Hour;
                var availableEndTime = scheduleInfo.WorkingHours.EndTime.Value.Hour;

                var tutorialTimeSlots = new List<TutorialTimeSlot>();

                var availableStartDateTime = startDate.AddHours(availableStartTime);
                var availableEndDateTime = startDate.AddHours(availableEndTime);
                var tutorialMinutes = 0;

                while(true)
                {
                    var tutorialStartDateTime = availableStartDateTime.AddMinutes(tutorialMinutes);
                    var tutorialEndDateTime = availableStartDateTime.AddMinutes(tutorialMinutes + tutorialDuration);

                    if (tutorialEndDateTime > availableEndDateTime)
                    {
                        //NOTE: Tutorial goes past working hours
                        break;
                    }

                    var isSlotAvailable = !scheduleInfo.ScheduleItems.Any(x => tutorialStartDateTime >= x.Start.ToDateTime() && tutorialEndDateTime <= x.End.ToDateTime());

                    if (isSlotAvailable) {

                        tutorialTimeSlots.Add(new TutorialTimeSlot()
                        {
                            StartTimeHours = tutorialStartDateTime.Hour,
                            StartTimeMinutes = tutorialStartDateTime.Minute,
                            EndTimeHours = tutorialEndDateTime.Hour,
                            EndTimeMinutes = tutorialEndDateTime.Minute
                        });
                    
                    }

                    tutorialMinutes += tutorialDuration;

                }

                response.Content.CostPerTimeSlot = costPerTimeSlot;
                response.Content.TutorialTimeSlots = tutorialTimeSlots;
                response.Status = ResponseStatus.OK;
                return response;
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
                            {
                                Message = ex.Message
                        }
                    };

                return response;
            }

        }

        [Route("api/app/customeractivity")]
        [HttpPost]
        public async Task<ResponseData<LessonHistoryResponseViewModel>> CustomerActivity(LessonHistoryRequestViewModel request)
        {

            var response = new ResponseData<LessonHistoryResponseViewModel>()
            {
                Content = new LessonHistoryResponseViewModel(),
            };

            try
            {

                var lessonHistory = await db.CustomerActivity
                .Include(x => x.CustomerDevice)
                .Include(x => x.Lesson)
                .Include(x => x.Lesson.LessonGroup)
                .OrderByDescending(x => x.StartDateTime)
                .Where(x => x.CustomerDevice.CustomerId == _customerId && x.Deleted == false)
                .Skip(request.Page * request.RecordCount)
                .Take(request.RecordCount)
                .ToListAsync();

                response.Content.LessonHistory = lessonHistory.Select(x => new LessonHistory()
                {
                    LessonId = x.LessonId,
                    LessonName = x.Lesson.Name,
                    LessonGroupName = x.Lesson.LessonGroup.Name,
                    DateTimeWatched = x.StartDateTime.ToString("dddd, dd MMMM yyyy @ HH:mm")
                }).ToList();
                response.Status = ResponseStatus.OK;

                return response;
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
                            {
                                Message = ex.Message
                            }
                        };

                return response;
            }
        }


        [Route("api/app/tutorialpurchase")]
        [HttpPost]
        public async Task<ResponseData<TutorialPurchaseResponseViewModel>> TutorialPurchase(TutorialPurchaseRequestViewModel request)
        {
            try
            {

                var response = new ResponseData<TutorialPurchaseResponseViewModel>();

                //Create Tutorial
                var customer = await db.Customer
                    .Include(x => x.App)
                    .FirstAsync(x => x.Id == _customerId);
                var tutor = await db.Users.FirstAsync(x => x.Id == request.TutorId);
                var tutorialSubject = await db.TutorialSubject.FirstAsync(x => x.Id == request.TutorialSubjectId);

                var customerFullName = string.Format("{0} {1}", customer.FirstName, customer.LastName);
                var teamsEventTutorialName = string.Format("DRAFT - Scottish Online Lessons Tutorial for {0}", customerFullName);
                var displayTutorialName = string.Format("{0} with {1}", tutorialSubject.Name, tutor.Firstname + " " + tutor.Surname);
                var tutorialDescription = new StringBuilder(string.Format("Scottish Online Lessons Tutorial for {0}", customerFullName));
                tutorialDescription.AppendLine("<p><b>Notes</b></p>");
                tutorialDescription.AppendLine("<p>" + request.CustomerNotes.Replace("\n", "<br />") + "</p>");

                Lesson lesson = null;
                if (request.LessonId.HasValue)
                    lesson = await db.Lesson
                        .Include(x => x.LessonGroup)
                        .FirstAsync(x => x.Id == request.LessonId.Value);

                if(lesson != null)
                {
                    tutorialDescription.AppendLine("<p><b>Related Lesson</b></p>");
                    tutorialDescription.AppendLine("<p>" + lesson.LessonGroup.Name + " - " + lesson.Name + "</p>");
                    tutorialDescription.AppendLine(string.Format("<p><a href=\"{0}api/lessonumbraco/{1}\">View Lesson</a></p>", _baseReturnUrl, lesson.Id));

                    request.CustomerNotes += "Related Lesson:" + System.Environment.NewLine;
                    request.CustomerNotes += lesson.LessonGroup.Name + " - " + lesson.Name + System.Environment.NewLine;
                    request.CustomerNotes += string.Format("{0}api/lessonumbraco/{1}", _baseReturnUrl, lesson.Id);
                }

                var graphApi = new MicrosoftGraphApiService();
                var teamsEventResponse = await graphApi.CreateOrUpdateTeamsEvent(
                    null,
                    tutor.Email,
                    teamsEventTutorialName,
                    tutorialDescription.ToString(),
                    _systemTimeZone,
                    request.DateTimeStart.ToString("s"),
                    request.DateTimeEnd.ToString("s"),
                    null
                    );

                var tutorialDuration = request.DateTimeEnd.Subtract(request.DateTimeStart).Minutes;

                var tutorial = new Tutorial()
                {
                    Name = displayTutorialName,
                    TeamsId = teamsEventResponse.Id,
                    TeamsLink = teamsEventResponse.WebLink,
                    AppId = 1,
                    CustomerId = customer.Id,
                    Notes = request.CustomerNotes,
                    LessonId = request.LessonId,
                    TutorialSubjectId = request.TutorialSubjectId,
                    DurationInMinutes = tutorialDuration,
                    DateTimeStart = request.DateTimeStart,
                    DateTimeEnd = request.DateTimeEnd,
                    TutorUserId = tutor.Id,
                    Deleted = false,
                    DateCreated = DateTime.UtcNow,
                    CreatedUserId = _adminUserId,
                    DateModified = DateTime.UtcNow,
                    ModifiedUserId = _adminUserId
                };

                string tutorialPriceId;

                switch (tutorialDuration)
                {
                    case 20:
                        tutorial.TutorialCost = customer.App.TutorialCost20Minutes;
                        tutorialPriceId = customer.App.TutorialStripPriceId20Minutes;
                        break;
                    case 40:
                        tutorial.TutorialCost = customer.App.TutorialCost40Minutes;
                        tutorialPriceId = customer.App.TutorialStripPriceId40Minutes;
                        break;
                    case 60:
                        tutorial.TutorialCost = customer.App.TutorialCost60Minutes;
                        tutorialPriceId = customer.App.TutorialStripPriceId60Minutes;
                        break;
                    default:
                        throw new Exception("Invalid Tutorial Duration");
                }

                db.Entry(tutorial).State = EntityState.Added;
                await db.SaveChangesAsync();


                var options = new SessionCreateOptions
                {
                    // NOTE: https://stripe.com/docs/api/checkout/sessions/create
                    ClientReferenceId = _tutorialStripePrefix + tutorial.Id.ToString(),
                    SuccessUrl = request.SuccessUrl + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = request.CancelUrl + "?session_id={CHECKOUT_SESSION_ID}",
                    PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                    Mode = "payment",
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = tutorialPriceId,
                            Quantity = 1,
                        },
                    },
                };
                var service = new SessionService(this.client);
                var session = await service.CreateAsync(options);

                tutorial.StripePaymentSessionId = session.Id;
                tutorial.HasCompletedCheckout = false;
                db.Entry(tutorial).State = EntityState.Modified;
                await db.SaveChangesAsync();

                response.Status = ResponseStatus.OK;
                response.Content = new TutorialPurchaseResponseViewModel
                {
                    SessionId = session.Id,
                };
                return response;

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

        [Route("api/app/lesson")]
        [HttpPost]
        public async Task<ResponseData<Lesson>> Lesson(LessonRequestViewModel lessonRequestViewModel)
        {
            var response = new ResponseData<Lesson>();

            var lesson = await db.Lesson.FirstOrDefaultAsync(x =>
            x.Id == lessonRequestViewModel.LessonId &&
            x.Deleted == false);

            response.Content = lesson;
            response.Status = ResponseStatus.OK;

            return response;

        }

        [Route("api/app/lessonmediaurl")]
        [HttpPost]
        public async Task<ResponseData<LessonStreamingResponse>> LessonMediaUrl(LessonRequestViewModel lessonRequestViewModel)
        {
            var response = new ResponseData<LessonStreamingResponse>();

            try
            {
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
                        response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
                            {
                                Code = ErrorCode.InvalidLicence,
                                Message = "Your licence does not allow access to this content. Please contact support"
                            }
                        };

                        return response;
                    }

                    var deviceRepsonse = await CheckCustomerDevice(lessonRequestViewModel.CustomerDevice);
                
                    if (deviceRepsonse.Status != ResponseStatus.OK)
                    {
                        response.Status = deviceRepsonse.Status;
                        response.ErrorResponse = deviceRepsonse.ErrorResponse;
                        response.Content = null;
                        return response;
                    }


                    //Log the view
                    var dbCustomerDevice = db.CustomerDevice.FirstOrDefault(x => x.DeviceIdentifier == lessonRequestViewModel.CustomerDevice.DeviceIdentifier); //This is now called twice, should be refactored

                    var customerActivity = new CustomerActivity()
                    {
                        CustomerDeviceId = dbCustomerDevice.Id,
                        LessonId = lesson.Id,
                        StartDateTime = DateTimeOffset.UtcNow,
                    };

                    db.CustomerActivity.Add(customerActivity);
                    await db.SaveChangesAsync();

                    if (lessonRequestViewModel.RemoteMediaType == RemoteMediaType.EncryptedStream || lessonRequestViewModel.RemoteMediaType == RemoteMediaType.StandardStream)
                    {
                        var lessonStreamingToken = new LessonStreamingToken()
                        {
                            LessonId = lesson.Id,
                            ExpiryDate = DateTime.UtcNow.AddHours(1)
                        };

                        var lessonStreamingTokenJson = JsonConvert.SerializeObject(lessonStreamingToken);
                        var encryptedData = cryptoNetKey.EncryptFromString(lessonStreamingTokenJson);
                        var encryptedToken = HttpServerUtility.UrlTokenEncode(encryptedData);

                        var url = string.Format("/VideoHandler.ashx?lessonId={0}&actionType=m3u8&token={1}", lesson.Id, encryptedToken);

                        response.Status = ResponseStatus.OK;
                        response.Content = new LessonStreamingResponse()
                        {
                            StreamingUrl = url,
                            Token = encryptedToken,
                        };

                    }
                    else if (lessonRequestViewModel.RemoteMediaType == RemoteMediaType.Download)
                    {
                        var lessonStreamingToken = new LessonStreamingToken()
                        {
                            LessonId = lesson.Id,
                            ExpiryDate = DateTime.UtcNow.AddHours(2)
                        };

                        var lessonStreamingTokenJson = JsonConvert.SerializeObject(lessonStreamingToken);
                        var encryptedData = cryptoNetKey.EncryptFromString(lessonStreamingTokenJson);
                        var encryptedToken = HttpServerUtility.UrlTokenEncode(encryptedData);

                        var videoSourcePath = Path.Combine(_videoRootFolder, lessonRequestViewModel.LessonId.ToString());
                        var downloadZipPath = Path.Combine(_videoDownloadFolder, lessonRequestViewModel.LessonId.ToString(), lessonRequestViewModel.LessonId.ToString() + ".zip");
       
                        if(!System.IO.File.Exists(downloadZipPath))
                            ZipService.ZipFolder(downloadZipPath, videoSourcePath);

                        var url = string.Format("/VideoHandler.ashx?lessonId={0}&actionType=download&token={1}", lesson.Id, encryptedToken);

                        response.Status = ResponseStatus.OK;
                        response.Content = new LessonStreamingResponse()
                        {
                            StreamingUrl = url,
                            Token = encryptedToken,
                        };

                    }
                    else
                    {
                        response.Status = ResponseStatus.Failed;
                        response.ErrorResponse = new List<ErrorResponse>()
                    {
                        new ErrorResponse()
                        {
                            Message = "Invalid Remote Media Type."
                        }
                    };

                        return response;

                    }


                }
                else
                {

                    response.Status = licenceResponse.Status;
                    response.ErrorResponse = licenceResponse.ErrorResponse;
                    response.Content = null;


                }

                return response;

            } catch(Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse()
                            {
                                Message = ex.Message
                            }
                        };

                return response;
            }

        }

        async Task<ResponseBase> CheckCustomerDevice(CustomerDeviceViewModel customerDevice)
        {

            var response = new ResponseBase();

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
                response.ErrorResponse = new List<ErrorResponse>()
                {
                    new ErrorResponse()
                    {
                        Message = string.Format("Your account already has {0} devices allocated to it. Please remove a device in 'My Account' to use this one.", customer.MaxDevicesAllowed)
                    }
                };
                return response;

            }

            //Add device
            db.CustomerDevice.Add(new CustomerDevice()
            {
                Name = customerDevice.Name,
                DeviceIdentifier = customerDevice.DeviceIdentifier,
                DeviceType = customerDevice.DeviceType,
                CustomerId = _customerId
            });

            await db.SaveChangesAsync();

            response.Status = ResponseStatus.OK;
            return response;
        }

        [Route("api/app/subscription")]
        [HttpPost]
        public async Task<ResponseData<Subscription>> CheckSubscription()
        {

            var response = new ResponseData<Subscription>();

            var subscription = await db.Subscription
                .OrderByDescending(x => x.EndDate)
                .FirstOrDefaultAsync(x => 
                    x.CustomerId == _customerId && 
                    x.Active == true);

            if (subscription == null)
            {
                response.Status = ResponseStatus.InvalidLicence;
                response.ErrorResponse = new List<ErrorResponse>()
                {
                    new ErrorResponse()
                    {
                        Code = ErrorCode.LicenceExpired,
                        Message = "A valid licence does not exist. Please subscribe to a new plan."
                    }
                };

                return response;

            }
            else if (subscription.EndDate < DateTime.Today)
            {
                response.Status = ResponseStatus.LicenceExpired;
                response.ErrorResponse = new List<ErrorResponse>()
                {
                    new ErrorResponse()
                    {
                           Code = ErrorCode.LicenceExpired,
                        Message = "Your licence has expired or has been cancelled. Please subscribe to a new plan."
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
        public async Task<ResponseData<bool>> SendEmail(SendEmailRequestViewModel request)
        {

            if(request.CreateFreeTrial)
            {
                return await CreateFreeTrail(request);
            }

            var response = new ResponseData<bool>();
            var graphApi = new MicrosoftGraphApiService();

            try
            {

                var html = string.Format("<p>{0}</p><p>{1}</p><p>{2}</p>", request.Name, request.Email, request.Message);


                await graphApi.SendEmail(_graphSenderAdminAccount, request.Subject, html, new List<string>() { "info@scottishonlinelessons.com" }, null, new List<string>() { "sysadmin@isai.co.uk" }, new List<string>() { _graphSenderAdminAccount }, true);
                response.Content = true;
                response.Status = ResponseStatus.OK;
              

            } catch(Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>() { new ErrorResponse () {
                        Message = ex.Message,
                        ErrorDescription = ex.StackTrace
                    }
                };
                return response;
            }

      
            return response;

        }


        async Task<SubscriptionCode> GenerateTrialCode(string code)
        {
            var subscriptionCode = new SubscriptionCode();
            subscriptionCode.Code = Guid.NewGuid().ToString();

            if (code.Equals("AUG30TRIAL")) {

                subscriptionCode.IssuedTo = "August 2021 - 30 Day Free Trial";
                subscriptionCode.SubscriptionTypeId = 1;
                subscriptionCode.ValidFrom = new DateTime(2021, 7, 1);
                subscriptionCode.ValidTo = new DateTime(2021, 9, 1);
                subscriptionCode.LicenceDays = 30;

            }
            else if (code.Equals("STCHARLESFREETRIAL"))
            {
                subscriptionCode.IssuedTo = "St Charles - 30 Day Free Trial";
                subscriptionCode.SubscriptionTypeId = 1;
                subscriptionCode.ValidFrom = new DateTime(2021, 7, 1);
                subscriptionCode.ValidTo = new DateTime(2021, 10, 8);
                subscriptionCode.LicenceDays = 30;

            }
            else
            {
                subscriptionCode.IssuedTo = "1 Day Free Trial";
                subscriptionCode.SubscriptionTypeId = 1;
                subscriptionCode.ValidFrom = DateTime.Today;
                subscriptionCode.ValidTo = DateTime.Now.AddDays(1);
                subscriptionCode.LicenceDays = 1;
            }

            db.Entry(subscriptionCode).State = EntityState.Added;
            await db.SaveChangesAsync();

            return subscriptionCode;
        }

        public async Task<ResponseData<bool>> CreateFreeTrail(SendEmailRequestViewModel request)
        {
            var response = new ResponseData<bool>();
            
            try
            {
                //TODO: not sure what this class does
                var code = await GenerateTrialCode("AUG30TRIAL");


                if (request.UseMobileForTrial)
                {


                    String message = HttpUtility.UrlEncode(string.Format("Hi {0}, Welcome to Scottish Online Lessons Free Trial. Your access code is:\n\n {1} \n\n Sign up here: https://scottishonlinelessons.com/plans/signup/", request.Name, code.Code));
                    using (var wb = new WebClient())
                    {
                        byte[] smsResponse = wb.UploadValues("https://api.txtlocal.com/send/", new NameValueCollection()
                    {
                    {"apikey" , "NzI2ZDM4MzA0NTM0NTk1MzY3NDc3MDM2NjQ3NjY0NTU="},
                    {"numbers" , "44" + request.Email.Trim().TrimStart("0".ToCharArray()) },
                    {"message" , message},
                    {"sender" , "Scottish Online Lessons"}
                    });
                        string result = System.Text.Encoding.UTF8.GetString(smsResponse);
                        //return result;


                        var html = string.Format("<p>{0}</p><p>{1}</p><p>{2}</p><p>User has been sent the following SMS message: <br />{3}</p>", request.Name, request.Email, request.Message, message);
                        var graphApi = new MicrosoftGraphApiService();
                        await graphApi.SendEmail(_graphSenderAdminAccount, request.Subject, html, new List<string>() { "info@scottishonlinelessons.com" }, null, new List<string>() { "sysadmin@isai.co.uk" }, new List<string>() { _graphSenderAdminAccount }, true);

                        response.Content = true;
                        response.Status = ResponseStatus.OK;

                        return response;
                    }

                }
                else
                {

                    //var clientId = "6ad5bfe4fdd0aed4f49c077d062dd58b";
                    var apiKey = "373gQZV6QcQAMEFTKtaVKYlOlEAE2qUloFWowSvdqF4hM1wbYNmPjbvASTeNiJ19yJOTsb8b8B7+LVUTBYHE2U+XsEXgbOyoZpcekmudIMfjQhJF5dG6LCWNyUqSB6vD95rgmD/iddmq55L99em2Dg==";
                    var listId = "666d699af51cdf4ba54574dd36553817";

                    AuthenticationDetails auth = new ApiKeyAuthenticationDetails(apiKey);
                    var general = new General(auth);
                    var clients = general.Clients();

                    Subscriber subscriber = new Subscriber(auth, listId);

                    try
                    {
                        List<SubscriberCustomField> customFields = new List<SubscriberCustomField>();
                        customFields.Add(new SubscriberCustomField() { Key = "Name", Value = request.Name });
                        customFields.Add(new SubscriberCustomField() { Key = "ActivationCode", Value = code.Code });

                        string newSubscriberID = subscriber.Add(request.Email, request.Name, customFields, false, ConsentToTrack.Unchanged);

                        var html = string.Format("<p>{0}</p><p>{1}</p><p>{2}</p><p>User has been added to Campaign Monitor</p>", request.Name, request.Email, request.Message);

                        var graphApi = new MicrosoftGraphApiService();
                        await graphApi.SendEmail(_graphSenderAdminAccount, request.Subject, html, new List<string>() { "info@scottishonlinelessons.com" }, null, new List<string>() { "sysadmin@isai.co.uk" }, new List<string>() { _graphSenderAdminAccount }, true);

                        response.Content = true;
                        response.Status = ResponseStatus.OK;

                        return response;


                    }
                    catch (CreatesendException ex)
                    {
                        ErrorResult error = (ErrorResult)ex.Data["ErrorResult"];
                        Console.WriteLine(error.Code);
                        Console.WriteLine(error.Message);

                        response.Status = ResponseStatus.Failed;
                        response.ErrorResponse = new List<ErrorResponse>() { new ErrorResponse () {
                                Message = ex.Message,
                                ErrorDescription = ex.StackTrace
                            }
                        };
                        return response;
                    }
                    catch (Exception ex)
                    {
                        // Handle some other failure
                        Console.WriteLine(ex.ToString());


                        response.Status = ResponseStatus.Failed;
                        response.ErrorResponse = new List<ErrorResponse>() { new ErrorResponse () {
                                Message = ex.Message,
                                ErrorDescription = ex.StackTrace
                            }
                        };
                        return response;
                    }


                }

            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>() { new ErrorResponse () {
                            Message = ex.Message,
                            ErrorDescription = ex.StackTrace
                        }
                    };
                return response;
            }



        }

        [Route("api/app/lessons")]
        [HttpPost]
        public async Task<ResponseData<List<Lesson>>> Lessons()
        {
            var response = new ResponseData<List<Lesson>>();

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
        public async Task<ResponseData<List<LessonGroup>>> LessonGroups()
        {

            var response = new ResponseData<List<LessonGroup>>();

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
        public async Task<ResponseData<List<Subscription>>> Subscriptions()
        {

            var response = new ResponseData<List<Subscription>>();

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
        public async Task<ResponseBase> CancelSubscriptions()
        {

            var response = new ResponseBase();

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
        public async Task<ResponseData<List<Subscription>>> RestoreSubscriptions()
        {
            var response = new ResponseData<List<Subscription>>();

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
        public async Task<ResponseData<List<CustomerDevice>>> CustomerDevices()
        {

            var response = new ResponseData<List<CustomerDevice>>();

            var devices = await db.CustomerDevice.Where(x =>
            x.CustomerId == _customerId &&
            x.Deleted == false
            ).ToListAsync();

            response.Content = devices;
            response.Status = ResponseStatus.OK;

            return response;
        }


        [Route("api/app/deletedevice")]
        [HttpPost]
        public async Task<ResponseBase> DeleteDevice(CustomerDevice customerDevice)
        {

            var response = new ResponseBase ();

            var device = await db.CustomerDevice.FirstOrDefaultAsync(x => x.Id == customerDevice.Id && x.Deleted == false);

            if(device != null)
            {
                device.Deleted = true;
                device.DateModified = DateTime.UtcNow;
                db.Entry(device).State = EntityState.Modified;

                await db.SaveChangesAsync();
            }

            response.Status = ResponseStatus.OK;

            return response;
        }

        [AllowAnonymous]
        [Route("api/app/sendresetpasswordemail")]
        [HttpPost]
        public async Task<ResponseBase> SendResetPasswordEmail(SendResetPasswordEmailViewModel model)
        {

            var response = new ResponseBase();
            var customer = await db.Customer.FirstOrDefaultAsync(x => x.AppId == model.AppId && x.Email.Trim().ToLower() == model.Email.Trim().ToLower());

            if(customer != null)
            {
                var digest = string.Format("{0}|{1}|{2}", customer.Id, customer.AppId, DateTime.UtcNow.AddMinutes(60).Ticks);

                byte[] key = Convert.FromBase64String(_base64Key);
                byte[] iv = Convert.FromBase64String(_base64Iv);
                string encryptedDigest = Strings.Encrypt(digest, key, iv);

                var passwordResetLink = _baseReturnUrl + "reset-password?digest=" + HttpUtility.UrlEncode(encryptedDigest);

                var templateHtml = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/Email Templates/ResetPasswordEmailTemplate.html"));
                templateHtml = templateHtml.Replace("{{name}}", string.Format("{0} {1}", customer.FirstName, customer.LastName));
                templateHtml = templateHtml.Replace("{{link}}", passwordResetLink);

                try
                {

                    MicrosoftGraphApiService graphService = new MicrosoftGraphApiService();
                    await graphService.SendEmail(
                            _graphSenderAdminAccount,
                            "Reset Password - Scottish Online Lessons",
                            templateHtml,
                            new List<string>() { customer.Email },
                            null,
                            new List<string>() { "sysadmin@isai.co.uk" },
                            new List<string>() { _graphSenderAdminAccount },
                            true);

                } catch (Exception ex)
                {
                    var t = true;
                }

            }

            response.Status = ResponseStatus.OK;
            return response;

        }

        [AllowAnonymous]
        [Route("api/app/updatepassword")]
        [HttpPost]
        public async Task<ResponseBase> UpdatePassword(UpdatePasswordViewModel model)
        {

            var response = new ResponseBase();

            byte[] key = Convert.FromBase64String(_base64Key);
            byte[] iv = Convert.FromBase64String(_base64Iv);
            var digest = Strings.Decrypt(model.Digest, key, iv).Split("|".ToCharArray());
            var customerId = Convert.ToInt32(digest[0]);
            var appId = Convert.ToInt32(digest[1]);
            var expiry = new DateTime(Convert.ToInt64(digest[2]), DateTimeKind.Utc);

            if(DateTime.UtcNow > expiry)
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                {
                    new ErrorResponse()
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
                response.ErrorResponse = new List<ErrorResponse>()
                {
                    new ErrorResponse()
                    {
                        Message = "Customer does not exist"
                    }
                };
                return response;
            }


            if (model.Password != model.ConfirmPassword)
            {
              
               response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                {
                    new ErrorResponse()
                    {
                        Message = "Passwords do not match"
                    }
                };
                return response;
            }

            if (!PasswordCheckService.IsStrongPassword(model.Password))
            {
                response.Status = ResponseStatus.Failed;
                response.ErrorResponse = new List<ErrorResponse>()
                {
                    new ErrorResponse()
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

                ICustomer customer;

                if (User.Identity.IsAuthenticated) { 

                    customer = await db.Customer.FirstAsync(x => x.Id == _customerId);
                }
                else
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
                    customer = register.Customer;
                }

                var subscriptionType = await db.SubscriptionType.FirstAsync(x => x.Id == request.SubscriptionTypeId);

                var options = new SessionCreateOptions
                {
                    // See https://stripe.com/docs/api/checkout/sessions/create
                    ClientReferenceId = customer.Id.ToString(),
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
                            Price = subscriptionType.StripePriceId,
                            Quantity = 1,
                        },
                    },
                };
                var service = new SessionService(this.client);

                var session = await service.CreateAsync(options);

                customer.PaymentSessionId = session.Id;
                db.Entry(customer).State = EntityState.Modified;
                await db.SaveChangesAsync();

                await CreateCustomerSubscription((Customer)customer, subscriptionType);

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
                    customer.HearAbout = model.HearAbout;

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
                        MaxDevicesAllowed = 2,
                        HearAbout = model.HearAbout
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



        [Route("api/app/signupaccesscode")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseData<Customer>> SignupAccessCode(RegisterRequestViewModel request)
        {
            var response = new ResponseData<Customer>();

            try
            {

                SubscriptionCode subscriptionCode = null;

                if (request.AccessCode.Trim().ToUpper().Equals("FREE1DAYTRIAL") || request.AccessCode.Trim().ToUpper().Equals("STCHARLESFREETRIAL"))
                {
                    subscriptionCode = await GenerateTrialCode(request.AccessCode.Trim().ToUpper());
                }
                else
                {

                    var accessCode = request.AccessCode.Trim().ToUpper();
                    subscriptionCode = await db.SubscriptionCode.FirstOrDefaultAsync(x =>
                        x.Code == accessCode &&
                        x.ValidFrom <= DateTime.UtcNow &&
                        x.UsedDateTime.HasValue == false);

                }

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
                response.ErrorResponse = new List<ErrorResponse>() { new ErrorResponse () {
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


        [Route("api/app/cancelstripesubscription")]
        [HttpPost]
        public async Task CancelStripeSubscription()
        {

            try
            {

                var subscription = await db.Subscription.FirstAsync(x => x.CustomerId == _customerId);

                var service = new SubscriptionService(this.client);
                var response = service.Cancel(subscription.StripeSubscriptionId); //TODO: handle this response

                //TODO: need to handled deleteing of scubscriptions as they will expoire after cancellation date
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;
                subscription.Deleted = true;
                subscription.Active = false;
                db.Entry(subscription).State = EntityState.Modified;

                var stripeWebhookLog = new StripeWebhookLog();
                stripeWebhookLog.CallBackName = "Stripe Subscription Cancelled by User";
                stripeWebhookLog.Description = string.Format("Subscription Cancelled for Customer {0} and Subscription {1}", subscription.CustomerId, subscription.StripeSubscriptionId);
                stripeWebhookLog.CreatedUserId = _adminUserId;
                stripeWebhookLog.ModifiedUserId = _adminUserId;
                stripeWebhookLog.DateCreated = DateTimeOffset.UtcNow;
                stripeWebhookLog.DateModified = DateTimeOffset.UtcNow;
                db.Entry(stripeWebhookLog).State = EntityState.Added;
                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //cancelStripeSubscription



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

            Stripe.Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    options.WebhookSecret
                );

                Console.WriteLine($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");

                var stripeWebhookLog = new StripeWebhookLog();
                stripeWebhookLog.CallBackName = "Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}";
                stripeWebhookLog.Description = stripeEvent.Data != null ? stripeEvent.Data.Object.ToString() : string.Empty;
                stripeWebhookLog.CreatedUserId = _adminUserId;
                stripeWebhookLog.ModifiedUserId = _adminUserId;
                stripeWebhookLog.DateCreated = DateTimeOffset.UtcNow;
                stripeWebhookLog.DateModified = DateTimeOffset.UtcNow;
                db.Entry(stripeWebhookLog).State = EntityState.Added;
                await db.SaveChangesAsync();

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

                    var checkOutComplete = stripeEvent.Data.Object as Session;

                    if (checkOutComplete.ClientReferenceId.StartsWith(_tutorialStripePrefix))
                    {
                        var tutorialId = Convert.ToInt32(checkOutComplete.ClientReferenceId.Replace(_tutorialStripePrefix, string.Empty));
                        var tutorial = await db.Tutorial
                            .Include(x => x.TutorUser)
                            .Include(x => x.Customer)
                            .FirstAsync(x => x.Id == tutorialId);
               

                        tutorial.HasCompletedCheckout = true;
                        tutorial.PendingEmailConfirmationTutor = true;
                        tutorial.PendingEmailConfirmationUser = true;
                        tutorial.StripePaymentId = checkOutComplete.PaymentIntentId;
                        tutorial.DateModified = DateTime.UtcNow;
                        db.Entry(tutorial).State = EntityState.Modified;

                        await db.SaveChangesAsync();

                        var customerFullName = string.Format("{0} {1}", tutorial.Customer.FirstName, tutorial.Customer.LastName);
                        var tutotialName = string.Format("CONFIRMED - Scottish Online Lessons Tutorial for {0}", customerFullName);
    
                        var graphApi = new MicrosoftGraphApiService();
                        var teamsEventResponse = await graphApi.CreateOrUpdateTeamsEvent(
                            tutorial.TeamsId,
                            tutorial.TutorUser.Email,
                            tutotialName,
                            null,
                            _systemTimeZone,
                            null,
                            null,
                            new List<Tuple<string, string>>()
                            {
                                new Tuple<string, string>(customerFullName, tutorial.Customer.Email)
                            }
                            );

                    }
                    else
                    {

                        customerId = Convert.ToInt32(checkOutComplete.ClientReferenceId);
                        var customer = await db.Customer.FirstAsync(x => x.Id == customerId);

                        //Add Stripe customer ID
                        customer.StripeCustomerId = checkOutComplete.CustomerId;
                        customer.HasCompletedCheckout = true;
                        customer.DateModified = DateTime.UtcNow;
                        db.Entry(customer).State = EntityState.Modified;

                        //Add Stripe Subscription
                        var subscription = await db.Subscription
                                .OrderByDescending(x => x.Id)
                                .FirstAsync(x => x.CustomerId == customer.Id && x.Deleted == false);

                        subscription.Active = true;
                        subscription.StripeSubscriptionId = checkOutComplete.SubscriptionId;
                        subscription.DateModified = DateTime.UtcNow;
                        db.Entry(subscription).State = EntityState.Modified;

                        await db.SaveChangesAsync();

                    }


                    break;
                case "invoice.paid":
                    // Continue to provision the subscription as payments continue to be made.
                    // Store the status in your database and check when a user accesses your service.
                    // This approach helps you avoid hitting rate limits.
                    var paidInvoice = stripeEvent.Data.Object as Stripe.Invoice;
                    await UpdateCustomerSubscription(paidInvoice.SubscriptionId, true, paidInvoice);

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
                case "payment_intent.succeeded":
                default:
                    // Unhandled event type
                    break;
            }

        }

        async Task CreateCustomerSubscription(Customer customer, SubscriptionType subscriptionType, SubscriptionCode subscriptionCode = null)
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
                //TODO we should do a change subscription first
                var subscription = await db.Subscription.FirstAsync(x => x.CustomerId == customer.Id && x.Deleted == false);
             
                if (subscription != null)
                {
                    subscription.Name = subscriptionType.Name;
                    subscription.Active = false;
                    subscription.StartDate = DateTime.UtcNow;
                    subscription.EndDate = DateTime.UtcNow;
                    subscription.SubscriptionTypeId = subscriptionType.Id;
                    subscription.DateModified = DateTime.UtcNow;
                    subscription.ModifiedUserId = _adminUserId;

                    db.Entry(subscription).State = EntityState.Modified;

                }
                else
                {
                    subscription = new Subscription()
                    {
                        Name = subscriptionType.Name,
                        CustomerId = customer.Id,
                        SubscriptionTypeId = subscriptionType.Id,
                        Active = false,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow,
                        DateModified = DateTime.UtcNow,
                        DateCreated = DateTime.UtcNow,
                        CreatedUserId = _adminUserId,
                        ModifiedUserId = _adminUserId
                    };

                    db.Subscription.Add(subscription);

                }

                customer.HasCompletedCheckout = false;
                customer.DateModified = DateTime.UtcNow;
                customer.ModifiedUserId = _adminUserId;

                db.Entry(customer).State = EntityState.Modified;

                await db.SaveChangesAsync();
            }

        }

        async Task UpdateCustomerSubscription(string subscriptionId, bool isActive, Invoice invoice = null)
        {

            var subscription = await db.Subscription
                .Include(x => x.SubscriptionType)
                .FirstAsync(x => x.StripeSubscriptionId == subscriptionId && x.Deleted == false);

            if (isActive == false)
            {
                //TODO: need to make sure Mangage area is fixed and send an email to user
                subscription.Active = false;
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;

                db.Entry(subscription).State = EntityState.Modified;

            } else {

                subscription.Active = true;
                subscription.EndDate = subscription.EndDate.AddMonths(subscription.SubscriptionType.SubscriptionLengthInMonths);
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;

                db.Entry(subscription).State = EntityState.Modified;

            }

            await db.SaveChangesAsync();

        }


    }


    #endregion

}