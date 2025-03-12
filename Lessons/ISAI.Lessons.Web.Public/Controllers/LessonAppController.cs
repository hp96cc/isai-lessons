using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.EntityFramework.ViewModels;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using ISAI.Lessons.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Tutorial = ISAI.Lessons.EntityFramework.Models.Tutorial;

namespace ISAI.Lessons.Web.Public.Controllers
{
    public class LessonAppController : ApiController, IAuthService
    {

        string _baseUrl;
        string _baseReturnUrl;

        const string _refreshCookieName = "lessons_refresh_token";
        const string _accessCookieName = "lessons_access_token";

        const int _appId = 1;

        ApiService _apiService;
        HttpClient _httpClient;
        HttpContext _context;

        public LessonAppController()
        {

            _baseUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.Portal.Url"];
            _baseReturnUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.ReturnUrl"];
            _context = HttpContext.Current;
            _apiService = new ApiService(this, _baseUrl, _baseReturnUrl);

        }


        [Route("api/lessonapp/login")]
        [HttpPost]
        public async Task Login(LoginRequestViewModel model) 
        {
            var auth = await GetAuthToken(model.Email, model.Password, null);

            if (auth == null)
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

        }

        [Route("api/lessonapp/checksession")]
        [HttpPost]
        public async Task<bool> CheckSession()
        {
            var isAuthenticated = await IsRequestAuthenticated();

            if (isAuthenticated == false) 
                Logout();

            return isAuthenticated;

        }


        [Route("api/lessonapp/logout")]
        [HttpPost]
        public void Logout()
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(_refreshCookieName, string.Empty)
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddDays(-1)
            });

            HttpContext.Current.Response.Cookies.Add(new HttpCookie(_accessCookieName, string.Empty)
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddDays(-1)
            });

        }

        [Route("api/lessonapp/customer")]
        [HttpPost]
        public async Task<ResponseData<Customer>> Customer()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/customer", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var customer = JsonConvert.DeserializeObject<ResponseData<Customer>>(serialisedContent);
                    return customer;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }


        [Route("api/lessonapp/cancelstripesubscription")]
        [HttpPost]
        public async Task CancelStripeSubscription()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/cancelstripesubscription", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var customer = JsonConvert.DeserializeObject<ResponseData<Customer>>(serialisedContent);
                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

        }



        [Route("api/lessonapp/subscription")]
        [HttpPost]
        public async Task<ResponseData<Subscription>> CheckSubscription()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/subscription", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<ResponseData<Subscription>>(serialisedContent);
                    return data;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

        }



        [Route("api/lessonapp/tutorialsubjectgroups")]
        [HttpPost]
        public async Task<ResponseData<TutorialSubjectGroupResponseViewModel>> GetTutorialSubjectGroups()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/tutorialsubjectgroups", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<ResponseData<TutorialSubjectGroupResponseViewModel>>(serialisedContent);
                    return data;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }


        [Route("api/lessonapp/tutorial")]
        [HttpPost]
        public async Task<ResponseData<Tutorial>> GetTutorial(TutorialRequestViewModel request)
        {

            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                var json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/tutorial", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<ResponseData<Tutorial>>(serialisedContent);
                    return data;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

        }

            [Route("api/lessonapp/tutorials")]
        [HttpPost]
        public async Task<ResponseData<TutorialResponseViewModel>> GetTutorials()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/tutorials", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<ResponseData<TutorialResponseViewModel>>(serialisedContent);
                    return data;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }


        [Route("api/lessonapp/tutorialsubjects")]
        [HttpPost]
        public async Task<ResponseData<TutorialSubjectResponseViewModel>> GetTutorialSubjectGroups(TutorialSubjectRequestViewModel request)
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                var json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/tutorialsubjects", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<ResponseData<TutorialSubjectResponseViewModel>>(serialisedContent);
                    return data;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/lessonapp/tutors")]
        [HttpPost]
        public async Task<ResponseData<TutorResponseViewModel>> GetTutors(TutorRequestViewModel request)
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                var json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/tutors", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<ResponseData<TutorResponseViewModel>>(serialisedContent);
                    return data;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/lessonapp/tutorialtimeslots")]
        [HttpPost]
        public async Task<ResponseData<TutorTimeSlotResponseViewModel>> GetTutotialTimeSlots(TutorTimeSlotRequestViewModel request)
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                var json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/tutorialtimeslots", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<ResponseData<TutorTimeSlotResponseViewModel>>(serialisedContent);
                    return data;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }



        [Route("api/lessonapp/tutorialpurchase")]
        [HttpPost]
        public async Task<ResponseData<TutorialPurchaseResponseViewModel>> TutorialPurchase(TutorialPurchaseRequestViewModel request)
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {

                request.CancelUrl = _baseReturnUrl + "/tutorials/payment-failed";
                request.SuccessUrl = _baseReturnUrl + "/tutorials/payment-success";

                var json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/tutorialpurchase", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var response = JsonConvert.DeserializeObject<ResponseData<TutorialPurchaseResponseViewModel>>(serialisedContent);
                    return response;
                }
                else
                {
                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }



        [Route("api/lessonapp/savecustomer")]
        [HttpPost]
        public async Task<ResponseData<Customer>> Customer(Customer customer)
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {

                var json = JsonConvert.SerializeObject(customer);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/savecustomer", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var savedCustomer = JsonConvert.DeserializeObject<ResponseData<Customer>>(serialisedContent);
                    return savedCustomer;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }




        [Route("api/lessonapp/lesson")]
        [HttpPost]
        public async Task<ResponseData<Lesson>> Lesson(LessonRequestViewModel model)
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {

                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/lesson", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var lesson = JsonConvert.DeserializeObject<ResponseData<Lesson>>(serialisedContent);
                    return lesson;
                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }


        [Route("api/lessonapp/customerdevices")]
        [HttpPost]
        public async Task<ResponseData<List<CustomerDevice>>> CustomerDevices()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/customerdevices", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var customerDevices = JsonConvert.DeserializeObject<ResponseData<List<CustomerDevice>>> (serialisedContent);
                    return customerDevices;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }


        [Route("api/lessonapp/deletedevice")]
        [HttpPost]
        public async Task<ResponseBase> DeleteDevice(CustomerDevice request)
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {

                var json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/deletedevice", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var customerDevices = JsonConvert.DeserializeObject<ResponseBase>(serialisedContent);
                    return customerDevices;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }




        [Route("api/lessonapp/customeractivity")]
        [HttpPost]
        public async Task<ResponseData<LessonHistoryResponseViewModel>> CustomerActivity(LessonHistoryRequestViewModel request)
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                var json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/customeractivity", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var customerActivity = JsonConvert.DeserializeObject<ResponseData<LessonHistoryResponseViewModel>>(serialisedContent);
                    return customerActivity;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/lessonapp/subscriptionportal")]
        [HttpPost]
        public async Task<Stripe.BillingPortal.Session> StripeCustomerPortal()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                var model = new StripeCustomerPortalRequest()
                {
                    ReturnUrl = _baseReturnUrl + "/account/subscriptions"
                };

                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/stripecustomerportal", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var billingPortalSession = JsonConvert.DeserializeObject<Stripe.BillingPortal.Session>(serialisedContent);
                    return billingPortalSession;

                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }




        [Route("api/lessonapp/sendresetpasswordemail")]
        [HttpPost]
        public async Task<ResponseBase> SendResetPasswordEmail(SendResetPasswordEmailViewModel model)
        {

            var response = new ResponseBase();
            SetHttpClient();

            try
            {
                model.AppId = _appId;

                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/sendresetpasswordemail", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    response = JsonConvert.DeserializeObject<ResponseBase>(serialisedContent);
                    return response;

                }
                else
                {
                    response.Status = ResponseStatus.Failed;
                }
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
            }

            return response;

        }


        [Route("api/lessonapp/updatepassword")]
        [HttpPost]
        public async Task<ResponseBase> UpdatePassword(UpdatePasswordViewModel model)
        {

            var response = new ResponseBase();
            SetHttpClient();

            try
            {
                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/updatepassword", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    response = JsonConvert.DeserializeObject<ResponseBase>(serialisedContent);
                    return response;

                }
                
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Failed;
            }

            return response;

        }

        [Route("api/lessonapp/lessonstreamurl")]
        [HttpPost]
        public async Task<ResponseData<LessonStreamingResponse>> GetLessonStreamingUrlAsync(LessonRequestViewModel model)
        {

            var response = new ResponseData<LessonStreamingResponse>();

            try
            {
                _httpClient = await _apiService.SetHttpAuthClient();

                var browser = new Kong.Browser(string.Join(" ", Request.Headers.GetValues("User-Agent")));

                var browserName = "";

                if(browser.Windows)
                {
                    browserName = "Microsoft Windows " + browser.OSVersion;
                } 
                else if(browser.Mac)
                {
                    browserName = "MacOS " + browser.OSVersion;
                }
                else if (browser.Linux)
                {
                    browserName = "Linux " + browser.OSVersion;
                }

                model.CustomerDevice.DeviceType = "Web";
                model.CustomerDevice.Name = string.Format("{0} ({1}) - {2}", browser.Name, browser.Version, browserName);
                model.RemoteMediaType = RemoteMediaType.EncryptedStream;

                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/lessonmediaurl", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var lessonStreamingResponse = JsonConvert.DeserializeObject<ResponseData<LessonStreamingResponse>>(serialisedContent);
                    return lessonStreamingResponse;

                }
                else
                {
                    var errorResponse = await ParseHttpError(httpResponse);
                    throw new Exception(errorResponse);
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

        [Route("api/lessonapp/signupaccesscode")]
        [HttpPost]
        public async Task<ResponseData<Customer>> SignupAccessCode(RegisterRequestViewModel model)
        {
            SetHttpClient();

            try
            {
                model.AppId = _appId;

                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/signupaccesscode", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var stripeCheckoutSessionResponse = JsonConvert.DeserializeObject<ResponseData<Customer>>(serialisedContent);
                    return stripeCheckoutSessionResponse;
                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

        }

        [AllowAnonymous]
        [Route("api/lessonapp/sendemail")]
        [HttpPost]
        public async Task<ResponseData<bool>> SendEmail(SendEmailRequestViewModel request)
        {
            SetHttpClient();

            var response = new ResponseData<bool>();


            try
            {
 
                var json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/sendemail", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var responseContent = JsonConvert.DeserializeObject<ResponseData<bool>>(serialisedContent);
                    return responseContent;
                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
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

        [Route("api/lessonapp/createcustomerpaymentsession")]
        [HttpPost]
        public async Task<CreateCustomerPayemntSessionResponse> CreateCustomerPaymentSession(CreateCustomerPayemntSessionRequest model)
        {

            if(model.IsSubscriptionChange)
            {
                _httpClient = await _apiService.SetHttpAuthClient();
            } else
            {
                SetHttpClient();
            }


            try
            {
                model.AppId = _appId;
                model.CancelUrl = _baseReturnUrl + "/plans/payment-cancel";

                if (model.IsSubscriptionChange)
                    model.SuccessUrl = _baseReturnUrl + "/account/subscriptions";
                else
                    model.SuccessUrl = _baseReturnUrl + "/plans/payment-success";


                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/stripecreatepayemntsession", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var stripeCheckoutSessionResponse = JsonConvert.DeserializeObject<CreateCustomerPayemntSessionResponse>(serialisedContent);
                    return stripeCheckoutSessionResponse;
                }
                else
                {
                    throw new HttpResponseException(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

        }





            #region Auth Methods

            async Task<Auth> GetAuthToken(string username, string password, string postRegistrationAccessCode)
        {
            var auth = await _apiService.GetAuthToken(username, password, postRegistrationAccessCode);
            return auth;
        }

        public async Task SetAuth(Auth auth)
        {

            if (auth != null)
            {
                _context.Response.Cookies.Add(new HttpCookie(_refreshCookieName, auth.RefreshToken)
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = true
                });

                _context.Response.Cookies.Add(new HttpCookie(_accessCookieName, auth.AccessToken)
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = true
                });
            }

        }


        public async Task<bool> IsRequestAuthenticated()
        {
            var auth = await _apiService.RefreshToken();

            if (auth != null && auth.AccessToken != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Auth> GetAuth()
        {

            var cookies = _context.Request.Cookies;

            if (cookies != null && cookies.Count > 0)
            {
                cookies.Get(_accessCookieName);

                var auth = new Auth();

                var accessCookie = cookies.Get(_accessCookieName);

                if (accessCookie != null)
                {
                    auth.AccessToken = accessCookie.Value;
                }

                var refreshCookie = cookies.Get(_refreshCookieName);

                if (refreshCookie != null)
                {
                    auth.RefreshToken = refreshCookie.Value;
                }

                return auth;

            }

            return null;
        }


        void SetHttpClient()
        {

            _httpClient = new HttpClient()
            {

                MaxResponseContentBufferSize = int.MaxValue,
                Timeout = TimeSpan.FromSeconds(30),
                BaseAddress = new Uri(_baseUrl)


            };

            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Keep-Alive", "true");

      
        }



        #endregion

        private async Task<string> ParseHttpError(HttpResponseMessage httpResponse)
        {

            var content = await httpResponse.Content.ReadAsStringAsync();
            return string.Format("HTTP Error Message: {0} : ErrorDescription: {1}", httpResponse.StatusCode.ToString(), content);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if(_httpClient != null) _httpClient.Dispose();
                _httpClient = null;
            }
            base.Dispose(disposing);

        }

        
    }
}