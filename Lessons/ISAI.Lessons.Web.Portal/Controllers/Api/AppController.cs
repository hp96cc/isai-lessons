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
using Microsoft.Extensions.Options;
using System.ClientModel.Primitives;
using Microsoft.Ajax.Utilities;
using RequestOptions = Stripe.RequestOptions;
using Syncfusion.EJ2.Diagrams;
using ImageResizer.ExtensionMethods;
using Azure.Core;


namespace ISAI.Lessons.Web.Portal.Controllers.Api
{

    [Authorize(Roles = "AppUser")]
    public class AppController : BaseApiController
    {

        int _customerId;
        int _appId;
        string _baseUrl;
        string _baseReturnUrl;

        StripeOptions optionsPlatform;
        StripeOptions optionsConnectedAccounts;
        IStripeClient client;
        IStripeClient clientConnectedAccounts;
        ICryptoNet cryptoNetKey;

        private readonly string _adminUserId = ConfigurationManager.AppSettings["SystemUserId"];
        private readonly string _base64Key = ConfigurationManager.AppSettings["Video.Base64Key"];
        private readonly string _base64Iv = ConfigurationManager.AppSettings["Video.Base64Iv"];
        private readonly string _aesKeyFile = ConfigurationManager.AppSettings["Video.AesKeyFile"];
        private readonly string _tutorialStripePrefix = ConfigurationManager.AppSettings["Stripe.TutorialPrefix"];
        private readonly string _groupTutorialStripePrefix = ConfigurationManager.AppSettings["Stripe.GroupTutorialPrefix"];
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

            this.optionsPlatform = new StripeOptions()
            {
                PublishableKey = ConfigurationManager.AppSettings["STRIPE_PUBLISHABLE_KEY"],
                SecretKey = ConfigurationManager.AppSettings["STRIPE_SECRET_KEY"],
                WebhookSecret = ConfigurationManager.AppSettings["STRIPE_WEBHOOK_SECRET_PLATFORM"],
            };

            this.optionsConnectedAccounts = new StripeOptions()
            {
                PublishableKey = ConfigurationManager.AppSettings["STRIPE_PUBLISHABLE_KEY"],
                SecretKey = ConfigurationManager.AppSettings["STRIPE_SECRET_KEY"],
                WebhookSecret = ConfigurationManager.AppSettings["STRIPE_WEBHOOK_SECRET_CONNECTEDACCOUNTS"],
            };

            _baseUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.Portal.Url"];
            _baseReturnUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.ReturnUrl"];

            this.client = new StripeClient(this.optionsPlatform.SecretKey);
            this.clientConnectedAccounts = new StripeClient(this.optionsConnectedAccounts.SecretKey);
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


        [Route("api/app/grouptutorialspublic")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<ResponseData<TutorialResponseViewModel>> GetGroupTutorialsPublic()
        {
            var response = new ResponseData<TutorialResponseViewModel>()
            {
                Content = new TutorialResponseViewModel()
            };

            try
            {
                var earlisetStartDate = DateTime.Today.AddDays(1);

                var groupTutorials = await db.GroupTutorial
                    .Include(x => x.TutorUser)
                    .Where(x => x.Deleted == false && x.DateTimeStart > earlisetStartDate)
                    .OrderBy(x => x.DateTimeStart)
                    .ToListAsync();

                var gmtStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_systemTimeZone);
                
                response.Content.Tutorials = groupTutorials.Select(x => new Lessons.Models.ViewModels.Tutorial()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsCompleted = x.DateTimeStart > DateTime.Now,
                    Date = TimeZoneInfo.ConvertTimeFromUtc(x.DateTimeStart.DateTime, gmtStandardTimeZone).ToString("dddd, dd MMMM yyyy"),
                    TimeStart = TimeZoneInfo.ConvertTimeFromUtc(x.DateTimeStart.DateTime, gmtStandardTimeZone).ToString("HH:mm"),
                    TimeEnd = TimeZoneInfo.ConvertTimeFromUtc(x.DateTimeEnd.DateTime, gmtStandardTimeZone).ToString("HH:mm"),
                    TeamsLink = x.TeamsLink,
                    TutorName = x.TutorUser.Firstname + " " + x.TutorUser.Surname,
                    IsUserSignedUp = false,
                    GroupTutorialGroupId = x.GroupTutorialGroupId,

                }).ToList();

                response.Content.GroupTutorialGroups = await db.GroupTutorialGroup
                                        .Where(x => x.Deleted == false)
                                        .OrderBy(x => x.ListOrder)
                                        .Select(x => new ISAI.Lessons.Models.ViewModels.GroupTutorialGroup()
                                        {
                                            Id = x.Id,
                                            Name = x.Name,
                                            ListOrder = x.ListOrder
                                        }).ToListAsync();

                foreach(var group in response.Content.GroupTutorialGroups)
                {
                    group.Tutorials = response.Content.Tutorials.Where(x => x.GroupTutorialGroupId == group.Id).ToList();    
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

        [Route("api/app/grouptutorials")]
        [HttpPost]
        public async Task<ResponseData<TutorialResponseViewModel>> GetGroupTutorials()
        {
            var response = new ResponseData<TutorialResponseViewModel>()
            {
                Content = new TutorialResponseViewModel()
            };

            try
            {
                var earlisetStartDate = DateTime.Today.AddDays(1);

                var groupTutorials = await db.GroupTutorial
                    .Include(x => x.TutorUser)
                    .Where(x => x.Deleted == false && x.DateTimeStart > earlisetStartDate)
                    .OrderBy(x => x.DateTimeStart)
                    .ToListAsync();

                var customerTutorials = await db.Tutorial.Where(x => x.CustomerId == _customerId &&
                                                        x.Deleted == false &&
                                                        x.HasCompletedCheckout &&
                                                        x.GroupTutorialId != null
                                                        ).ToListAsync();

                var gmtStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_systemTimeZone);


                response.Content.Tutorials = groupTutorials.Select(x => new Lessons.Models.ViewModels.Tutorial()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsCompleted = x.DateTimeStart > DateTime.Now,
                    Date = TimeZoneInfo.ConvertTimeFromUtc(x.DateTimeStart.DateTime, gmtStandardTimeZone).ToString("dddd, dd MMMM yyyy"),
                    TimeStart = TimeZoneInfo.ConvertTimeFromUtc(x.DateTimeStart.DateTime, gmtStandardTimeZone).ToString("HH:mm"),
                    TimeEnd = TimeZoneInfo.ConvertTimeFromUtc(x.DateTimeEnd.DateTime, gmtStandardTimeZone).ToString("HH:mm"),
                    TeamsLink = x.TeamsLink,
                    TutorName = x.TutorUser.Firstname + " " + x.TutorUser.Surname,
                    IsUserSignedUp = customerTutorials.Any(y => y.GroupTutorialId == x.Id)

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

        [Route("api/app/grouptutorial")]
        [HttpPost]
        public async Task<ResponseData<GroupTutorial>> GetGroupTutorial(int groupTutorialId)
        {
            var response = new ResponseData<GroupTutorial>();

            try
            {
                var groupTutorial = await db.GroupTutorial
                    .Where(x => x.Id == groupTutorialId)
                    .FirstAsync();

                response.Content = groupTutorial;
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

                SessionService service;
                Session session;

                if (string.IsNullOrWhiteSpace(request.TutorStripeId))
                {
                    service = new SessionService(this.client);
                    session = service.Get(request.StripeSessionId);
                } 
                else
                {
                    var requestOptions = new Stripe.RequestOptions
                    {
                        StripeAccount = request.TutorStripeId,
                    };
                    service = new SessionService(this.clientConnectedAccounts);
                    session = service.Get(request.StripeSessionId, null, requestOptions);

                }
                

                int tutorialId;

                if (session.ClientReferenceId.StartsWith(_groupTutorialStripePrefix))
                {
                    tutorialId = Convert.ToInt32(session.ClientReferenceId.Replace(_groupTutorialStripePrefix, string.Empty));
                } else
                {
                    tutorialId = Convert.ToInt32(session.ClientReferenceId.Replace(_tutorialStripePrefix, string.Empty));
                }


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
                var lastMonth = DateTime.Today.AddDays(-31);

                var tutorials = await db.Tutorial
                    .Include(x => x.TutorUser)
                    .Where(x => x.CustomerId == _customerId && x.Deleted == false && x.HasCompletedCheckout  && x.DateTimeStart > lastMonth)
                    .OrderByDescending(x => x.DateTimeStart)
                    .ToListAsync();

                var gmtStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_systemTimeZone);

  
                response.Content.Tutorials = tutorials.Select(x => new Lessons.Models.ViewModels.Tutorial()
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsCompleted = x.DateTimeEnd > DateTime.Now,
                    Date = TimeZoneInfo.ConvertTimeFromUtc(x.DateTimeStart.DateTime, gmtStandardTimeZone).ToString("dddd, dd MMMM yyyy"),
                    TimeStart = TimeZoneInfo.ConvertTimeFromUtc(x.DateTimeStart.DateTime, gmtStandardTimeZone).ToString("HH:mm"),
                    TimeEnd = TimeZoneInfo.ConvertTimeFromUtc(x.DateTimeEnd.DateTime, gmtStandardTimeZone).ToString("HH:mm"),
                    TeamsLink = x.TeamsLink,
                    TutorName = x.TutorUser.Firstname + " " + x.TutorUser.Surname,
                    GroupTutorialId = x.GroupTutorialId
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
                        .Where(x => x.TutorUser.Deleted == false && x.TutorialSubjectId == request.TutorialSubjectId && !string.IsNullOrEmpty(x.TutorUser.TutorEmail) && x.TutorUser.TutorStripeId != null && x.TutorUser.TutorStripeId.Length > 5)
                        .Select(x => new Tutor()
                        {
                            Id = x.TutorUser.Id,
                            Name = x.TutorUser.Firstname + " " + x.TutorUser.Surname,
                            TutorStripeId = x.TutorUser.TutorStripeId
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
                       .Where(x => x.TutorUser.Deleted == false && x.TutorialSubjectId == lesson.LessonGroup.TutorialSubjectId && !string.IsNullOrEmpty(x.TutorUser.TutorEmail) && x.TutorUser.TutorStripeId != null && x.TutorUser.TutorStripeId.Length > 5)
                       .Select(x => new Tutor()
                       {
                           Id = x.TutorUser.Id,
                           Name = x.TutorUser.Firstname + " " + x.TutorUser.Surname,
                           TutorStripeId = x.TutorUser.TutorStripeId
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

                var tutorialTimeSlots = new List<TutorialTimeSlot>();

                if (scheduleInfo.WorkingHours == null)
                {
                    var message = string.Format("Tutor {0} has not set any working hours. They will not be available until this has been done", tutorUser.Email);
                    await SendEmail(new SendEmailRequestViewModel()
                    {
                        Email = tutorUser.Email,
                        Message = message,
                        Name = tutorUser.Fullname,
                        Subject = "Tutor Working Hours not setup"

                    });
                } 
                else 
                { 

                    var availableStartTime = scheduleInfo.WorkingHours.StartTime.Value.Hour;
                    var availableEndTime = scheduleInfo.WorkingHours.EndTime.Value.Hour;

                    var availableStartDateTime = startDate.AddHours(availableStartTime);

                    var minStartingDate = DateTime.Now.AddHours(8);
                    if (minStartingDate.Day == DateTime.Today.Day && minStartingDate > availableStartDateTime)
                    {
                        availableStartDateTime = DateTime.Today.AddHours(minStartingDate.Hour);
                    }

                    var availableEndDateTime = startDate.AddHours(availableEndTime);
                    var tutorialMinutes = 0;

                    while (true)
                    {
                        var tutorialStartDateTime = availableStartDateTime.AddMinutes(tutorialMinutes);
                        var tutorialEndDateTime = availableStartDateTime.AddMinutes(tutorialMinutes + tutorialDuration);

                        if (tutorialEndDateTime > availableEndDateTime)
                        {
                            //NOTE: Tutorial goes past working hours
                            break;
                        }

                        var isSlotAvailable = !scheduleInfo.ScheduleItems.Any(x =>
                            (tutorialStartDateTime < x.End.ToDateTime() &&
                            x.Start.ToDateTime() < tutorialEndDateTime));


                        if (isSlotAvailable)
                        {

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
                                Message = ex.Message + ex.StackTrace
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

                var lastMonth = DateTime.Today.AddDays(-31);

                var lessonHistory = await db.CustomerActivity
                .Include(x => x.CustomerDevice)
                .Include(x => x.Lesson)
                .Include(x => x.Lesson.LessonGroup)
                .OrderByDescending(x => x.StartDateTime)
                .Where(x => x.CustomerDevice.CustomerId == _customerId && x.Deleted == false && x.StartDateTime > lastMonth)
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


        [Route("api/app/grouptutorialpurchase")]
        [HttpPost]
        public async Task<ResponseData<GroupTutorialPurchaseResponseViewModel>> GroupTutorialPurchase(GroupTutorialPurchaseRequestViewModel request)
        {
            try
            {

                var response = new ResponseData<GroupTutorialPurchaseResponseViewModel>();

                //Create Tutorial
                var customer = await db.Customer
                    .Include(x => x.App)
                    .FirstAsync(x => x.Id == _customerId);

                var groupTutorial = await db.GroupTutorial
                    .Include(x => x.TutorUser)
                    .FirstAsync(x => x.Id == request.GroupTutorialId);

                var tutor = groupTutorial.TutorUser;

                var tutorialDuration = Convert.ToInt32((groupTutorial.DateTimeEnd.Ticks - groupTutorial.DateTimeStart.Ticks) / TimeSpan.TicksPerMinute);


                var tutorial = await db.Tutorial.FirstOrDefaultAsync(x => x.GroupTutorialId == groupTutorial.Id && x.CustomerId == customer.Id);

                if (tutorial != null)
                {
                    if (tutorial.HasCompletedCheckout == true && !string.IsNullOrEmpty(tutorial.StripePaymentId))
                    {
                        response.Status = ResponseStatus.Failed;
                        response.ErrorResponse = new List<ErrorResponse>()
                        {
                            new ErrorResponse() {
                                Message = "You have already joined this tutorial."
                            }
                        };

                        return response;
                    }

                }
                else
                {

                    tutorial = new Tutorial()
                    {
                        Name = groupTutorial.Name,
                        TeamsId = groupTutorial.TeamsId,
                        TeamsLink = groupTutorial.TeamsLink,
                        TutorialCost = 15,
                        AppId = 1,
                        GroupTutorialId = groupTutorial.Id,
                        CustomerId = customer.Id,
                        DurationInMinutes = tutorialDuration,
                        DateTimeStart = groupTutorial.DateTimeStart,
                        DateTimeEnd = groupTutorial.DateTimeEnd,
                        TutorUserId = tutor.Id,
                        Deleted = false,
                        DateCreated = DateTime.UtcNow,
                        CreatedUserId = _adminUserId,
                        DateModified = DateTime.UtcNow,
                        ModifiedUserId = _adminUserId
                    };

                    db.Entry(tutorial).State = EntityState.Added;
                    await db.SaveChangesAsync();

                }


                var options = new SessionCreateOptions
                {
                    // NOTE: https://stripe.com/docs/api/checkout/sessions/create
                    ClientReferenceId = _groupTutorialStripePrefix + tutorial.Id.ToString(),
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
                            Price = customer.App.GroupTutorialStripePriceId, 
                            Quantity = 1,
                        },
                    }

                };

                var service = new SessionService(this.client);
                var session = await service.CreateAsync(options);

                tutorial.StripePaymentSessionId = session.Id;
                tutorial.HasCompletedCheckout = false;
                db.Entry(tutorial).State = EntityState.Modified;
                await db.SaveChangesAsync();

                response.Status = ResponseStatus.OK;
                response.Content = new GroupTutorialPurchaseResponseViewModel
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


                var gmtStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_systemTimeZone);

                var startDate = TimeZoneInfo.ConvertTimeFromUtc(request.DateTimeStart, gmtStandardTimeZone);
                var endDate = TimeZoneInfo.ConvertTimeFromUtc(request.DateTimeEnd, gmtStandardTimeZone);

                var graphApi = new MicrosoftGraphApiService();
                var teamsEventResponse = await graphApi.CreateOrUpdateTeamsEvent(
                    null,
                    tutor.Email,
                    teamsEventTutorialName,
                    tutorialDescription.ToString(),
                    _systemTimeZone,
                    startDate.ToString("s"),
                    endDate.ToString("s"),
                    null
                    );



                var tutorialDuration = Convert.ToInt32((endDate.Ticks - startDate.Ticks) / TimeSpan.TicksPerMinute);

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
                decimal tutorialFee;

                switch (tutorialDuration)
                {
                    case 20:
                        tutorial.TutorialCost = customer.App.TutorialCost20Minutes;
                        tutorialPriceId = tutor.TutorialStripPriceId20Minutes;
                        tutorialFee = customer.App.TutorialStripApplicationFee20Minutes;
                        break;
                    case 40:
                        tutorial.TutorialCost = customer.App.TutorialCost40Minutes;
                        tutorialPriceId = tutor.TutorialStripPriceId40Minutes;
                        tutorialFee = customer.App.TutorialStripApplicationFee40Minutes;
                        break;
                    case 60:
                        tutorial.TutorialCost = customer.App.TutorialCost60Minutes;
                        tutorialPriceId = tutor.TutorialStripPriceId60Minutes;
                        tutorialFee = customer.App.TutorialStripApplicationFee60Minutes;
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
                    SuccessUrl = request.SuccessUrl + "?tutorStripeId=" + tutor.TutorStripeId + "&session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = request.CancelUrl + "?tutorStripeId=" + tutor.TutorStripeId + "&session_id={CHECKOUT_SESSION_ID}",
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
                    
                    PaymentIntentData = new SessionPaymentIntentDataOptions
                    {
                        ApplicationFeeAmount = Convert.ToInt32(tutorialFee * 100),

                    }
                    
                };

                var requestOptions = new RequestOptions
                {
                    StripeAccount = tutor.TutorStripeId,
                };


                var service = new SessionService(this.clientConnectedAccounts);
                var session = await service.CreateAsync(options, requestOptions);

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
        [Route("api/app/lessondownload")]
        [AllowAnonymous]
        [HttpPost]
        public ResponseData<LessonStreamingResponse> LessonDownload(LessonRequestViewModel lessonRequestViewModel)
        {
            var response = new ResponseData<LessonStreamingResponse>();


            var lessonStreamingToken = new LessonStreamingToken()
            {
                LessonId = lessonRequestViewModel.LessonId,
                ExpiryDate = DateTime.UtcNow.AddHours(2)
            };

            var lessonStreamingTokenJson = JsonConvert.SerializeObject(lessonStreamingToken);
            var encryptedData = cryptoNetKey.EncryptFromString(lessonStreamingTokenJson);
            var encryptedToken = HttpServerUtility.UrlTokenEncode(encryptedData);

            var videoSourcePath = Path.Combine(_videoRootFolder, lessonRequestViewModel.LessonId.ToString());
            var downloadZipPath = Path.Combine(_videoDownloadFolder, lessonRequestViewModel.LessonId.ToString(), lessonRequestViewModel.LessonId.ToString() + ".zip");
            var downloadFolderPath = Path.Combine(_videoDownloadFolder, lessonRequestViewModel.LessonId.ToString());

            if (!System.IO.File.Exists(downloadZipPath))
            {

                if (!Directory.Exists(downloadFolderPath))
                    Directory.CreateDirectory(downloadFolderPath);

                ZipService.ZipFolder(downloadZipPath, videoSourcePath);

            }

            var url = string.Format("/VideoHandler.ashx?lessonId={0}&actionType=download&token={1}", lessonRequestViewModel.LessonId, encryptedToken);

            response.Status = ResponseStatus.OK;
            response.Content = new LessonStreamingResponse()
            {
                StreamingUrl = url,
                Token = encryptedToken,
            };

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

                    bool canAccessLesson;

                    var lesson = await db.Lesson.FirstOrDefaultAsync(x =>
                        x.Id == lessonRequestViewModel.LessonId &&
                        x.Deleted == false);

                    switch (licenceResponse.Content.SubscriptionTypeId)
                    {
                        case 1:
                        case 100:
                        case 101:
                            canAccessLesson = true;
                            break;

                        case 2:
                        case 102:
                        case 103:
                            canAccessLesson = lesson.SubscriptionTypeId == 2;
                            break;

                        case 3:
                        case 104:
                        case 105:
                            canAccessLesson = lesson.SubscriptionTypeId == 3;
                            break;

                        default:
                            canAccessLesson = false;
                            break;
                    }


                    if (!canAccessLesson)
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
                    var dbCustomerDevice = db.CustomerDevice.FirstOrDefault(x => x.CustomerId == _customerId && x.DeviceIdentifier == lessonRequestViewModel.CustomerDevice.DeviceIdentifier); //This is now called twice, should be refactored

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
                        var downloadZipPath = Path.Combine(_videoDownloadFolder, lessonRequestViewModel.LessonId.ToString() + ".zip");
       
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


        async Task<SubscriptionCode> GenerateTrialCode()
        {
            var subscriptionCode = new SubscriptionCode();
            subscriptionCode.Code = Guid.NewGuid().ToString();

            subscriptionCode.IssuedTo = "1 Day Free Trial";
            subscriptionCode.SubscriptionTypeId = 1;
            subscriptionCode.ValidFrom = DateTime.Today;
            subscriptionCode.ValidTo = DateTime.Now.AddDays(1);
            subscriptionCode.LicenceDays = 1;

            db.Entry(subscriptionCode).State = EntityState.Added;
            await db.SaveChangesAsync();

            return subscriptionCode;
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
                    var lessons = await db.Lesson.Where(x => x.Deleted == false && x.PendingDownload == false).ToListAsync();

                    response.Content = lessons;
                    response.Status = ResponseStatus.OK;
                    return response;
                }
                else
                {
                    var lessons = await db.Lesson.Where(x => x.Deleted == false && x.PendingDownload == false && x.SubscriptionTypeId == x.SubscriptionTypeId).ToListAsync();

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

        [Route("api/app/customersignup")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<CreateCustomerPayemntSessionResponse> CustomerSignup(CreateCustomerPayemntSessionRequest request)
        {
            try
            {
                //TODO: where we are changing a stripe subscription we will need to cancel on Strpe at the same time.
                var subscriptionType = await db.SubscriptionType.FirstAsync(x => x.Id == request.SubscriptionTypeId);

                Customer customer;

                if (User.Identity.IsAuthenticated)
                {

                    customer = await db.Customer.FirstAsync(x => x.Id == _customerId);

                    var didChangeWithoutCharge = await ChangeSubscriptionIfNoExtraCharge(subscriptionType);

                    if (didChangeWithoutCharge)
                    {
                        return new CreateCustomerPayemntSessionResponse
                        {
                            SessionId = "NOCHARGE",
                        };
                    }

                }
                else
                {
                    var register = await RegisterCustomer(request);


                    if (register.Content == null)
                    {
                        return new CreateCustomerPayemntSessionResponse()
                        {
                            SessionId = null,
                            Errors = register.ErrorResponse.Select(x => x.Message).ToList()
                        };
                    }
                    customer = register.Content;
                }



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
                    ReasonPhrase = ex.Message,
                    Content = new StringContent(ex.StackTrace)
                };

                if (ex.InnerException != null)
                {
                    errorMessage.Content = new StringContent(ex.StackTrace + ex.InnerException.StackTrace);
                }

                throw new HttpResponseException(errorMessage);


            }

        }


        #region Stripe Integration

        [Route("api/app/stripecreatepayemntsession")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<CreateCustomerPayemntSessionResponse> CreateCustomerPayemntSession(CreateCustomerPayemntSessionRequest request)
        {
            try
            {
                //TODO: where we are changing a stripe subscription we will need to cancel on Strpe at the same time.
                var subscriptionType = await db.SubscriptionType.FirstAsync(x => x.Id == request.SubscriptionTypeId);

                ICustomer customer;

                if (User.Identity.IsAuthenticated) { 

                    customer = await db.Customer.FirstAsync(x => x.Id == _customerId);

                    var didChangeWithoutCharge = await ChangeSubscriptionIfNoExtraCharge(subscriptionType);

                    if(didChangeWithoutCharge)
                    {
                        return new CreateCustomerPayemntSessionResponse
                        {
                            SessionId = "NOCHARGE",
                        };
                    }

                }
                else
                {
                    var register = await RegisterCustomer(request);

                    if (register.Content == null)
                    {
                        return new CreateCustomerPayemntSessionResponse()
                        {
                            SessionId = null,
                            Errors = register.ErrorResponse.Select(x => x.Message).ToList()
                        };
                    }
                    customer = register.Content;
                }

               

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
                    ReasonPhrase = ex.Message,
                    Content = new StringContent(ex.StackTrace)
                };

                if(ex.InnerException != null)
                {
                    errorMessage.Content = new StringContent(ex.StackTrace + ex.InnerException.StackTrace);
                }

                throw new HttpResponseException(errorMessage);


            }

        }

        async Task<bool> ChangeSubscriptionIfNoExtraCharge(SubscriptionType subscriptionType)
        {
            var validSubscriptions = await Subscriptions();

            if (validSubscriptions.Content.Count > 0)
            {
                var currentSubscription = validSubscriptions.Content[0];

                if (
                    currentSubscription.SubscriptionTypeId == 102 && subscriptionType.Id == 104 ||
                    currentSubscription.SubscriptionTypeId == 104 && subscriptionType.Id == 102 ||
                    currentSubscription.SubscriptionTypeId == 2 && subscriptionType.Id == 104 ||
                    currentSubscription.SubscriptionTypeId == 3 && subscriptionType.Id == 102)
                {
                    currentSubscription.SubscriptionType = subscriptionType;
                    currentSubscription.DateModified = DateTime.UtcNow;
                    currentSubscription.ModifiedUserId = _adminUserId;
                    db.Entry(currentSubscription).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return true;

                }

            }

            return false;

        }

        [Route("api/app/registercustomer")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseData<Customer>> RegisterCustomer(RegisterRequestViewModel model)
        {
            var response = new ResponseData<Customer>();
            var customer = await db.Customer.FirstOrDefaultAsync(x => x.AppId == model.AppId && x.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));
            var errors = new List<string>();


            if (customer != null)
            {
                errors.Add("User already exists. Please login.");
            }

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



                await db.SaveChangesAsync();

                response.Status = ResponseStatus.OK;
                response.Content = customer;
                return response;

            }

            response.Status = ResponseStatus.Failed;
            response.ErrorResponse = new List<ErrorResponse>();
            response.ErrorResponse = errors.Select(x => new ErrorResponse()
            {
                Message = x
            }).ToList();
            return response;

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

                if (request.AccessCode.Trim().ToUpper().Equals("FREE1DAYTRIAL"))
                {
                    subscriptionCode = await GenerateTrialCode();
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

                    if (register.Content == null)
                    {
                        if(register.ErrorResponse != null && register.ErrorResponse.Count > 0)
                        {
                            throw new Exception(register.ErrorResponse[0].Message);
                        }
                        else
                        {
                            throw new Exception("Could not create customer");
                        }
                       
                    }

                    await CreateCustomerSubscription((Customer)register.Content, null, subscriptionCode);

                    response.Content = (Customer)register.Content;
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


                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;

                try
                {
                    var service = new SubscriptionService(this.client);
                    var response = service.Cancel(subscription.StripeSubscriptionId);

                    //TODO: subscriptioin is kept active to end of billing period. Need to change this on background task 
                    subscription.Deleted = false;
                    subscription.Active = true; 

                }
                catch (Exception ex)
                {
                    //TODO: we dont know the state of Stripe here so we need to cancel immediateley.
                    subscription.Deleted = true;
                    subscription.Active = false; 
                }

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
                    json.Contains("\"" + _tutorialStripePrefix) ? optionsConnectedAccounts.WebhookSecret : optionsPlatform.WebhookSecret,
                    throwOnApiVersionMismatch: false
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
                        tutorial.PendingReviewEmailConfirmationUser = true;
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

                    else if (checkOutComplete.ClientReferenceId.StartsWith(_groupTutorialStripePrefix))
                    {
                        var tutorialId = Convert.ToInt32(checkOutComplete.ClientReferenceId.Replace(_groupTutorialStripePrefix, string.Empty));
                        var tutorial = await db.Tutorial
                            .Include(x => x.TutorUser)
                            .Include(x => x.Customer)
                            .FirstAsync(x => x.Id == tutorialId);

                        tutorial.HasCompletedCheckout = true;
                        tutorial.PendingEmailConfirmationTutor = false; //NOTE: we dont email the tutor for group tutorials
                        tutorial.PendingEmailConfirmationUser = true;
                        tutorial.PendingReviewEmailConfirmationUser = true;
                        tutorial.StripePaymentId = checkOutComplete.PaymentIntentId;
                        tutorial.DateModified = DateTime.UtcNow;
                        db.Entry(tutorial).State = EntityState.Modified;

                        await db.SaveChangesAsync();


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

                        //Add stripe Subcription Id
                        subscription.StripeSubscriptionId = checkOutComplete.SubscriptionId;
                        subscription.DateModified = DateTime.UtcNow;
                        db.Entry(subscription).State = EntityState.Modified;

                        var subscriptionType = await db.SubscriptionType.FirstAsync(x => x.Id == subscription.SubscriptionTypeId);
                        await db.SaveChangesAsync();

                        //check sequencing
                        await UpdateCustomerSubscription(checkOutComplete.SubscriptionId, true);

                    }


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
                    EndDate = subscriptionCode.ValidFrom.AddDays(subscriptionCode.LicenceDays), 
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
                //TODO: where we are changing a stripe subscription we will need to cancel on Strpe at the same time.

                //TODO we should do a change subscription first
                var subscriptions = await Subscriptions();

                if(subscriptions.Content != null && subscriptions.Content.Count > 0)
                {
                    //shoudl we ever update a subscription or just create a new one?
                    var subscriptionId = subscriptions.Content[0].Id;
                    var subscription = await db.Subscription.FirstOrDefaultAsync(x => x.CustomerId == customer.Id && x.Deleted == false);

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
                    var subscription = new Subscription()
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
                    db.Entry(subscription).State = EntityState.Added;

                }

                //TODO: what is change subscription fails?
                customer.HasCompletedCheckout = false;
                customer.DateModified = DateTime.UtcNow;
                customer.ModifiedUserId = _adminUserId;

                db.Entry(customer).State = EntityState.Modified;

                await db.SaveChangesAsync();
            }

        }

        async Task UpdateCustomerSubscription(string stripeSubscriptionId, bool isActive)
        {

            var subscription = await db.Subscription
                .Include(x => x.SubscriptionType)
                .FirstAsync(x => x.StripeSubscriptionId == stripeSubscriptionId && x.Deleted == false);

            //TODO: 1 day period is added to make sure Subscriptions dont get deleted when being setup. If so, we will just ignore
            if (isActive == false && subscription.DateCreated < DateTime.Today.AddDays(-1))
            {
                //TODO: need to make sure Mangage area is fixed and send an email to user
                subscription.Active = false;
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;

                db.Entry(subscription).State = EntityState.Modified;
                await db.SaveChangesAsync();

            } else {

                subscription.Active = true;

                subscription.EndDate = DateTime.Today.AddMonths(GetSubscriptionRenewalLength(subscription));
                subscription.DateModified = DateTime.UtcNow;
                subscription.ModifiedUserId = _adminUserId;

                db.Entry(subscription).State = EntityState.Modified;
                await db.SaveChangesAsync();

            }
          
        }

        private int GetSubscriptionRenewalLength(Subscription subscription)
        {

            if(subscription.SubscriptionType.StripePriceId !=  null)
                return subscription.SubscriptionType.SubscriptionLengthInMonths;

            //Handle Legacy Subscription Types where length was not part of Subscription Type
            if (subscription.Name.ToLower().Contains("annual"))
                 return 12;
            else
                return 1;
        }

    }


    #endregion

}