using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.Core.Services
{
    public class MicrosoftGraphApiService
    {

        readonly string baseAuthAddress = "https://login.microsoftonline.com/";
        readonly string tennantId = "72b8f033-67c4-4524-964e-d3c01efa1acd";
        readonly string authPath = "oauth2/v2.0/token";
        readonly string appId = "88030882-6822-4b8d-b8ae-19fe43c38f89";
        readonly string appSecret = "-u~-5YFpS0Q884b8~uW3Sc3oDSRy4gt7gg";
        readonly string scope = "https://graph.microsoft.com/.default";
        readonly string grantType = "client_credentials";

        string _authToken;

        public  MicrosoftGraphApiService()
        {

        }

        public async Task<IDriveItemChildrenCollectionPage> GetFilesForFolder(string driveId, string folderId)
        {
            GraphServiceClient graphClient = await GetAuthenticatedClient();
            var driveItems = await graphClient.Drives[driveId].Items[folderId].Children.Request().GetAsync();
            return driveItems;
        }

        public async Task SendEmail(string userName, string subject, string htmlBody, List<string> addressTo, List<string> addressCC, List<string> addressBCC, bool saveToSentItems = true)
        {
            GraphServiceClient graphClient = await GetAuthenticatedClient();

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = htmlBody
                },
                ToRecipients = addressTo.Select(x => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = x
                    }
                }).ToList()
            };

            if (addressCC != null && addressCC.Count > 0)
            {
                message.CcRecipients = addressCC.Select(x => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = x
                    }
                }).ToList();
            }

            if (addressBCC != null && addressBCC.Count > 0)
            {
                message.BccRecipients = addressBCC.Select(x => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = x
                    }
                }).ToList();
            }
            await graphClient.Users[userName]
                .SendMail(message, saveToSentItems)
                .Request()
                .PostAsync();


        }

        async Task<GraphServiceClient> GetAuthenticatedClient()
        {

            await GetAccessToken();

            GraphServiceClient graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        string accessToken = _authToken;
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                    }));
            return graphClient;
        }

        async Task GetAccessToken()
        {

            using (HttpClient httpClient = new HttpClient()
            {
                MaxResponseContentBufferSize = int.MaxValue,
                Timeout = TimeSpan.FromSeconds(30),
                BaseAddress = new Uri(baseAuthAddress)
            })

            {

                System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Keep-Alive", "true");

                var requestBody = new List<KeyValuePair<string, string>>();
                requestBody.Add(new KeyValuePair<string, string>("client_id", appId));
                requestBody.Add(new KeyValuePair<string, string>("scope", scope));
                requestBody.Add(new KeyValuePair<string, string>("client_secret", appSecret));
                requestBody.Add(new KeyValuePair<string, string>("grant_type", grantType));

                Uri path = new Uri(string.Format("{0}/{1}/{2}", baseAuthAddress, tennantId, authPath));

                var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(requestBody) };
                var httpResponse = await httpClient.SendAsync(request);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(serialisedContent);
                    _authToken = jsonObject.access_token;

                }

            }

        }
    }
}
