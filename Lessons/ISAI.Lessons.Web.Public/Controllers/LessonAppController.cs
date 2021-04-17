using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using ISAI.Lessons.EntityFramework.ViewModels;
using ISAI.Lessons.EntityFramework.ViewModels.Stripe;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ISAI.Lessons.Web.Public.Controllers
{
    public class LessonAppController : ApiController
    {

        string _baseUrl;
        string _baseReturnUrl;
        const string _refreshCookieName = "lessons_refresh_token";
        const string _accessCookieName = "lessons_access_token";
        const int _appId = 1;

        ApiService _apiService;
        HttpClient _httpClient;
        Auth _auth;

        public LessonAppController()
        {
            _baseUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.Portal.Url"];
            _baseReturnUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.ReturnUrl"];

            _apiService = new ApiService();
        }

        [Route("api/lessonapp/login")]
        [HttpPost]
        public async Task Login(LoginRequestViewModel model)
        {

            await GetAuthToken(HttpContext.Current, model.Email, model.Password, null);

            if (_auth == null)
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
            _httpClient = await _apiService.SetHttpAuthClient(_auth);

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
            _httpClient = await _apiService.SetHttpAuthClient(_auth);

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
            _httpClient = await _apiService.SetHttpAuthClient(_auth);

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
            _httpClient = await _apiService.SetHttpAuthClient(_auth);

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
            _httpClient = await _apiService.SetHttpAuthClient(_auth);

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
            _httpClient = await _apiService.SetHttpAuthClient(_auth);

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
            _httpClient = await _apiService.SetHttpAuthClient(_auth);

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


        [Route("api/lessonapp/createcustomerpaymentsession")]
        [HttpPost]
        public async Task<CreateCustomerPayemntSessionResponse> CreateCustomerPaymentSession(CreateCustomerPayemntSessionRequest model)
        {
            SetHttpClient();

            try
            {
                model.AppId = _appId;
                model.PriceId = "price_1IbJ0pJ81SbG6nzaIrTZGt25";
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

            var context = HttpContext.Current;

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

                    await GetAuthToken(context, customer.Email, string.Empty, customer.PostRegistrationAccessCode);

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

        async Task GetAuthToken(HttpContext context, string username, string password, string postRegistrationAccessCode)
        {

            _auth = await _apiService.GetAuthToken(username, password, postRegistrationAccessCode);

            if (_auth != null)
            {
                context.Response.Cookies.Add(new HttpCookie(_refreshCookieName, _auth.RefreshToken)
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = true
                });

                context.Response.Cookies.Add(new HttpCookie(_accessCookieName, _auth.AccessToken)
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = true
                });
            }

        }

        async Task RefreshToken()
        {

            string refreshToken = null;
            var cookies = Request.Headers.GetCookies();

            if (cookies != null && cookies.Count > 0)
            {

                var refreshCookie = cookies[0].Cookies.FirstOrDefault(x => x.Name.Equals(_refreshCookieName));

                if (refreshCookie != null)
                {
                    refreshToken = refreshCookie.Value;
                }

            }


            if (refreshToken != null)
            {

                _auth = await _apiService.RefreshToken(new Auth()
                {
                    RefreshToken = refreshToken
                });

                if (_auth != null)
                {
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(_refreshCookieName, _auth.RefreshToken)
                    {
                        Path = "/",
                        HttpOnly = true,
                        Secure = true
                    });

                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(_accessCookieName, _auth.AccessToken)
                    {
                        Path = "/",
                        HttpOnly = true,
                        Secure = true
                    });
                }

            }

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