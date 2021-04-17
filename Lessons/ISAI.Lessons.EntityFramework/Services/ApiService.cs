using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.ViewModels;
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

namespace ISAI.Lessons.EntityFramework.Services
{
    public class ApiService
    {

        string _baseUrl;
        string _baseReturnUrl;
        const string _refreshCookieName = "lessons_refresh_token";
        const string _accessCookieName = "lessons_access_token";
        const int _appId = 1;

        HttpClient _httpClient;
        Auth _auth;

        public ApiService()
        {
            _baseUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.Portal.Url"];
            _baseReturnUrl = ConfigurationManager.AppSettings["ISAI.Lessons.Web.ReturnUrl"];
        }

        public async Task<Auth> GetAuthToken(string username, string password, string postRegistrationAccessCode)
        {

            _auth = null;

            var client = new HttpClient();
            client.BaseAddress = new Uri(_baseUrl);
            var request = new HttpRequestMessage(HttpMethod.Post, "token");

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));
            keyValues.Add(new KeyValuePair<string, string>("username", username));
            keyValues.Add(new KeyValuePair<string, string>("password", password));

            if (postRegistrationAccessCode != null)
            {
                keyValues.Add(new KeyValuePair<string, string>("postRegistrationAccessCode", postRegistrationAccessCode));
            }

            keyValues.Add(new KeyValuePair<string, string>("appid", _appId.ToString()));

            request.Content = new FormUrlEncodedContent(keyValues);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {

                var serialisedContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _auth = JsonConvert.DeserializeObject<Auth>(serialisedContent);
                return _auth;

            }

            return null;

        }

        public async Task<Auth> RefreshToken(Auth auth)
        {

            var client = new HttpClient();
            client.BaseAddress = new Uri(_baseUrl);
            var request = new HttpRequestMessage(HttpMethod.Post, "token");

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            keyValues.Add(new KeyValuePair<string, string>("refresh_token", auth.RefreshToken));
            keyValues.Add(new KeyValuePair<string, string>("appid", _appId.ToString()));

            request.Content = new FormUrlEncodedContent(keyValues);

            Console.WriteLine("RefreshToken ATTEMPT - {0}", auth.RefreshToken);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {

                var serialisedContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                auth = JsonConvert.DeserializeObject<Auth>(serialisedContent);
                return auth;

            }

            return null;            

        }


        public async Task<HttpClient> SetHttpAuthClient(Auth auth)
        {

            if (auth == null) await RefreshToken(auth);

            if (auth != null && auth.AccessToken != null)
            {

                var httpClient = new HttpClient()
                {

                    MaxResponseContentBufferSize = int.MaxValue,
                    Timeout = TimeSpan.FromSeconds(30),
                    BaseAddress = new Uri(_baseUrl)


                };

                ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Keep-Alive", "true");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _auth.AccessToken);

                return httpClient;

            }

            return null;


        }



    }
}
