using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        const string _baseUrl = "https://localhost:44392/";
        const string _refreshCookieName = "lessons_refresh_token";
        const string _accessCookieName = "lessons_access_token";
        const string _appId = "1";

        HttpClient _httpClient;
        Auth _auth;

        public LessonAppController()
        {

        }

        [Route("api/lessonapp/login")]
        [HttpPost]
        public async Task Login(LoginRequestViewModel model)
        {

            await GetAuthToken(model.Email, model.Password);

            if (_auth == null)
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

        }


        [Route("api/lessonapp/register")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<RegisterResponseViewModel> Register(RegisterRequestViewModel model)
        {
            return null;

        }

        [Route("api/lessonapp/customer")]
        [HttpPost]
        public async Task<Customer> Customer()
        {
            await SetHttpAuthClient();

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
            await SetHttpAuthClient();

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
            await SetHttpAuthClient();

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
            await SetHttpAuthClient();

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
            await SetHttpAuthClient();

            try
            {
                HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/app/customerdevices", null).ConfigureAwait(false);

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

        [Route("api/lessonapp/forgotpassword")]
        [HttpPost]
        public async Task ForgotPassword()
        {
            await SetHttpAuthClient();

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


        #region Auth Methods



        async Task GetAuthToken(string username, string password)
        {

            _auth = null;

            var client = new HttpClient();
            client.BaseAddress = new Uri(_baseUrl);
            var request = new HttpRequestMessage(HttpMethod.Post, "token");

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));
            keyValues.Add(new KeyValuePair<string, string>("username", username));
            keyValues.Add(new KeyValuePair<string, string>("password", password));
            keyValues.Add(new KeyValuePair<string, string>("appid", _appId));
            
            request.Content = new FormUrlEncodedContent(keyValues);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {

                var serialisedContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _auth = JsonConvert.DeserializeObject<Auth>(serialisedContent);

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

                var client = new HttpClient();
                client.BaseAddress = new Uri(_baseUrl);
                var request = new HttpRequestMessage(HttpMethod.Post, "token");

                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
                keyValues.Add(new KeyValuePair<string, string>("refresh_token", refreshToken));
                keyValues.Add(new KeyValuePair<string, string>("appid", _appId));

                request.Content = new FormUrlEncodedContent(keyValues);

                Console.WriteLine("RefreshToken ATTEMPT - {0}", refreshToken);

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {

                    var serialisedContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _auth = JsonConvert.DeserializeObject<Auth>(serialisedContent);

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

        }


        async Task SetHttpAuthClient()
        {

            if(_auth == null) await RefreshToken();

            if (_auth != null && _auth.AccessToken != null)
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
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _auth.AccessToken);
              
            }


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