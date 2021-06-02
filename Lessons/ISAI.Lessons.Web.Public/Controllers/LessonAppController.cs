using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.EntityFramework.ViewModels;
using ISAI.Lessons.EntityFramework.ViewModels.Stripe;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using ISAI.Lessons.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

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

            if (_baseUrl.Contains("localhost"))
            {
                _apiService = new ApiService(true, this);
            }
            else
            {
                _apiService = new ApiService(false, this);
            }


        }


        [Route("api/lessonapp/test")]
        [HttpGet]
        public async Task<Customer> Test()
        {
            _httpClient = await _apiService.SetHttpAuthClient();


           HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/customer", null).ConfigureAwait(false);


            var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var customer = JsonConvert.DeserializeObject<Customer>(serialisedContent);
            return customer;

    
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
        public async Task<Customer> Customer()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/customer", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var customer = JsonConvert.DeserializeObject<Customer>(serialisedContent);
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


        [Route("api/lessonapp/lesson")]
        [HttpPost]
        public async Task<Lesson> Lesson(int lessonId)
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                var model = new LessonRequestViewModel()
                {
                    LessonId = lessonId
                };

                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/lesson", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var lesson = JsonConvert.DeserializeObject<Lesson>(serialisedContent);
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


        [Route("api/lessonapp/subscriptions")]
        [HttpPost]
        public async Task<List<Subscription>> Subscriptions()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/subscriptions", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var subscriptions = JsonConvert.DeserializeObject<List<Subscription>>(serialisedContent);
                    return subscriptions;

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
        public async Task<List<CustomerDevice>> CustomerDevices()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/customerdevices", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var customerDevices = JsonConvert.DeserializeObject<List<CustomerDevice>>(serialisedContent);
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
        public async Task<List<CustomerActivity>> CustomerActivity()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/customeractivity", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var customerActivity = JsonConvert.DeserializeObject<List<CustomerActivity>>(serialisedContent);
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




        [Route("api/lessonapp/forgotpassword")]
        [HttpPost]
        public async Task ForgotPassword()
        {
            _httpClient = await _apiService.SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/forgotpassword", null).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    return;
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

        [Route("api/lessonapp/lessonstreamurl")]
        [HttpPost]
        public async Task<ResponseData<LessonStreamingResponse>> GetLessonStreamingUrlAsync(LessonRequestViewModel model)
        {

            var response = new ResponseData<LessonStreamingResponse>();

            try
            {
                _httpClient = await _apiService.SetHttpAuthClient();

                var request = new LessonRequestViewModel()
                {
                    LessonId = model.LessonId
                };

                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/lessonstreamurl", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var lessonStreamingResponse = JsonConvert.DeserializeObject<LessonStreamingResponse>(serialisedContent);

                    response.Status = ResponseStatus.OK;
                    response.Content = lessonStreamingResponse;
                    return response;
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
        public async Task<Lessons.Models.Models.ResponseData<Customer>> SignupAccessCode(RegisterRequestViewModel model)
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
                    var stripeCheckoutSessionResponse = JsonConvert.DeserializeObject<Lessons.Models.Models.ResponseData<Customer>>(serialisedContent);
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

        [Route("api/lessonapp/createcustomerpaymentsession")]
        [HttpPost]
        public async Task<CreateCustomerPayemntSessionResponse> CreateCustomerPaymentSession(CreateCustomerPayemntSessionRequest model)
        {
            SetHttpClient();

            try
            {
                model.AppId = _appId;

                //LIVE id: 
                //monthly: price_1IxXUFJ81SbG6nzav7NG3Vto
                //annual: price_1IxXTvJ81SbG6nzajuriP6XZ

                model.CancelUrl = _baseReturnUrl + "/plans/payment-cancel";
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


        [Route("api/lessonapp/confirmcustomersubscription")]
        [HttpPost]
        public async Task<Customer> ConfirmCustomerSubscription(ConfirmCustomerSubscriptionRequest request)
        {

            SetHttpClient();

            try
            {
                request.AppId = _appId;

                var json = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/confirmcustomersubscription", content).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var customer = JsonConvert.DeserializeObject<Customer>(serialisedContent);

                    await GetAuthToken(customer.Email, string.Empty, customer.PostRegistrationAccessCode);

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

            throw new HttpResponseException(HttpStatusCode.InternalServerError);

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