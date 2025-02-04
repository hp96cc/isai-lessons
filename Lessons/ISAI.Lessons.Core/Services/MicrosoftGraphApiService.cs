using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph.Users.Item.SendMail;
using Microsoft.Graph.Users.Item.Calendar.GetSchedule;
using ISAI.Lessons.Models.ViewModels.GraphApi;
using System.Configuration;
using Azure;

namespace ISAI.Lessons.EntityFramework.Services
{
    public class MicrosoftGraphApiService
    {

        private readonly string baseAuthAddress = ConfigurationManager.AppSettings["MicrosoftGraph.BaseAuthAddress"];
        private readonly string tennantId = ConfigurationManager.AppSettings["MicrosoftGraph.TennantId"];
        private readonly string authPath = ConfigurationManager.AppSettings["MicrosoftGraph.AuthPath"];
        private readonly string appId = ConfigurationManager.AppSettings["MicrosoftGraph.AppId"];
        private readonly string appSecret = ConfigurationManager.AppSettings["MicrosoftGraph.AppSecret"];
        private readonly string scope = ConfigurationManager.AppSettings["MicrosoftGraph.Scope"];
        private readonly string grantType = ConfigurationManager.AppSettings["MicrosoftGraph.GrantType"];
        private readonly string graphSenderAdminAccount = ConfigurationManager.AppSettings["MicrosoftGraph.SenderEmail"];

        //NOTE: For Subscriptions and keys below see: https://learn.microsoft.com/en-us/graph/change-notifications-with-resource-data
        private readonly string publicSubscriptionKeyId = ConfigurationManager.AppSettings["MicrosoftGraph.PublicSubscriptionKeyId"];
        private readonly string publicSubscriptionKey = ConfigurationManager.AppSettings["MicrosoftGraph.PublicSubscriptionKey"];

        private string authToken;

        public MicrosoftGraphApiService() { }

        public async Task SendEmail(
            string userName, 
            string subject, 
            string htmlBody, 
            List<string> addressTo,
            List<string> addressCC, 
            List<string> addressBCC, 
            List<string> addressReplyTo, 
            bool saveToSentItems = true, 
            List<FileAttachment> attachments = null)
        {
            GraphServiceClient graphClient = GetAuthenticatedClient();

            if (addressCC == null) addressBCC = new List<string>();
            if (addressReplyTo == null) addressReplyTo = new List<string>();

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = htmlBody
                },
                ReplyTo = addressReplyTo.Select(x => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = x
                    }
                }).ToList(),


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

            if (attachments != null && attachments.Count > 0)
            {
                message.Attachments = new List<Attachment>();
                message.HasAttachments = true;

                foreach (var attachment in attachments)
                {
                    message.Attachments.Add(attachment);
                }
            }



            var requestBody = new SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = true,
            };


            await graphClient.Users[userName]
                             .SendMail
                             .PostAsync(requestBody);


        }


        public async Task<TeamsEventResponse> CreateOrUpdateTeamsEvent(
            string eventId,
            string userName,
            string subject,
            string htmlBody,
            string timeZone,
            string dateTimeStart,
            string dateTimeEnd,
            List<Tuple<string, string>> attendees)
        {
            GraphServiceClient graphClient = GetAuthenticatedClient();

            if (eventId != null)
            {
                var response = await graphClient.Users[userName].Events[eventId].GetAsync(requestConfiguration =>
                {
                    requestConfiguration.Headers.Add("Prefer", "outlook.timezone=\"" + timeZone + "\"");
                });

                if (subject != null)
                {
                    response.Subject = subject;
                }

                if (htmlBody != null)
                {
                    response.Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = htmlBody,
                    };
                }

                if (dateTimeStart != null)
                {
                    response.Start = new DateTimeTimeZone
                    {
                        DateTime = dateTimeStart,
                        TimeZone = timeZone,
                    };
                }

                if (dateTimeEnd != null)
                {
                    response.End = new DateTimeTimeZone
                    {
                        DateTime = dateTimeEnd,
                        TimeZone = timeZone,
                    };
                }

                if (attendees != null && attendees.Count > 0)
                {
                    response.Attendees = attendees.Select(x => new Attendee()
                    {
                        EmailAddress = new EmailAddress()
                        {
                            Name = x.Item1,
                            Address = x.Item2
                        },
                        Type = AttendeeType.Required,
                    }).ToList();
                }

                response.AllowNewTimeProposals = false;
                response.IsOnlineMeeting = true;
                response.OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness;



                var result = await graphClient.Users[userName].Events[eventId].PatchAsync(response, (requestConfiguration) =>
                {
                    requestConfiguration.Headers.Add("Prefer", "outlook.timezone=\"" + timeZone + "\"");
                });



                return new TeamsEventResponse()
                {
                    Id = result.Id,
                    WebLink = result.OnlineMeeting.JoinUrl
                };

            }
            else
            {
                var requestBody = new Event
                {
                    Subject = subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = htmlBody,
                    },
                    Start = new DateTimeTimeZone
                    {
                        DateTime = dateTimeStart,
                        TimeZone = timeZone,
                    },
                    End = new DateTimeTimeZone
                    {
                        DateTime = dateTimeEnd,
                        TimeZone = timeZone,
                    },

                    AllowNewTimeProposals = false,
                    IsOnlineMeeting = true,

                    OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness,
                };

                if (attendees != null && attendees.Count > 0)
                {

                    requestBody.Attendees = attendees.Select(x => new Attendee()
                    {
                        EmailAddress = new EmailAddress()
                        {
                            Name = x.Item1,
                            Address = x.Item2
                        },
                        Type = AttendeeType.Required,
                    }).ToList();

                }


                var eventResponse = await graphClient.Users[userName].Events.PostAsync(requestBody, (requestConfiguration) =>
                {
                    requestConfiguration.Headers.Add("Prefer", "outlook.timezone=\"" + timeZone + "\"");
                });

                return new TeamsEventResponse()
                {
                    Id = eventResponse.Id,
                    WebLink = eventResponse.OnlineMeeting.JoinUrl
                };


            }

        }

        public async Task<GetSchedulePostResponse> GetAvailability(string userName, List<string> emailAddresses, DateTime startDate, DateTime endDate, string timeZone, int interval)
        {
       
            GraphServiceClient graphClient = GetAuthenticatedClient();
      

            var requestBody = new GetSchedulePostRequestBody
            {
                Schedules = emailAddresses,
                StartTime = new DateTimeTimeZone
                {
                    DateTime = startDate.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                    TimeZone = timeZone,
                },
                EndTime = new DateTimeTimeZone
                {
                    DateTime = endDate.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                    TimeZone = timeZone,
                },
                AvailabilityViewInterval = interval,
            };

            try
            {
                var scheduleInformation = new List<ScheduleInformation>();

                // To initialize your graphClient, see https://learn.microsoft.com/en-us/graph/sdks/create-client?from=snippets&tabs=csharp
                var response = await graphClient.Users[userName].Calendar.GetSchedule.PostAsGetSchedulePostResponseAsync(requestBody, (requestConfiguration) =>
                {
                    requestConfiguration.Headers.Add("Prefer", "outlook.timezone=\"" + timeZone + "\"");
                });

                return response;

            } 
            catch(Exception ex)
            {
                //TODO: handle and log
                throw ex;
            }

        }

        public async Task<string> GetUserIdFromEmail(string email)
        {
            GraphServiceClient graphClient = GetAuthenticatedClient();
            User user = await graphClient.Users[email].GetAsync();
            return user.Id;
        }

        public async Task CreateSubscription(string notificationHost, string userId)
        {

            try
            {
                GraphServiceClient graphClient = GetAuthenticatedClient();

                await DeleteAllSubscriptions(graphClient, false);

                var subscription = new Subscription
                {
                    ChangeType = "created",
                    NotificationUrl = $"{notificationHost}/api/graph/listen",
                    LifecycleNotificationUrl = $"{notificationHost}/api/graph/lifecycle",
                    Resource = $"/users/{userId}/mailfolders/inbox/messages?$select=id,sender,subject",
                    ClientState = Guid.NewGuid().ToString(),
                    IncludeResourceData = true,
                    ExpirationDateTime = DateTimeOffset.UtcNow.AddDays(2),
                    EncryptionCertificateId = publicSubscriptionKeyId,
                    EncryptionCertificate = publicSubscriptionKey
                };

                var newSubscription = await graphClient.Subscriptions.PostAsync(subscription);

                if (newSubscription == null)
                {
                    Console.WriteLine("No subscription was returned.");
                }

                Console.WriteLine(newSubscription.Id);
                Console.WriteLine(newSubscription.ClientState);

            }
            catch (Exception ex)
            {
                var t = true;
            }


        }

        public async Task GetMessage()
        {
            GraphServiceClient graphClient = GetAuthenticatedClient();

            string resourceData = "Users('accd9cfc-a69e-48c5-ab1f-79826d8a25bf')/messages('AQMkAGNiN2MwNjlhLWVmOGMtNDUzZi04OWIwLWE3MTU3YjcyNDkxNwBGAAADdyOqrp1IjEG6gXV7aJGKwAcAEBPFgGk3aUixSCvqfODd6wAAAgEMAAAAEBPFgGk3aUixSCvqfODd6wAGd_7IkAAAAA==')";
            var resourceDataArray = resourceData.Split('/');
            var userId = resourceDataArray[0].TrimStart("Users('".ToCharArray()).TrimEnd("')".ToCharArray());
            var messageId = resourceDataArray[1].TrimStart("messages('".ToCharArray()).TrimEnd("')".ToCharArray());

            var message = await graphClient.Users[userId].Messages[messageId].GetAsync();

            if (message.HasAttachments ?? false)
            {
                var attachaments = await graphClient
                    .Users[userId]
                    .Messages[messageId]
                    .Attachments
                    .GetAsync();

                foreach (Attachment attachment in attachaments.Value)
                {
                    if (attachment is FileAttachment fileAttachment)
                        System.IO.File.WriteAllBytes(@"c:\temp\" + fileAttachment.Name, fileAttachment.ContentBytes);

                }

            }

            Console.WriteLine(message.Body.Content);

        }

        private async Task DeleteAllSubscriptions(GraphServiceClient graphClient, bool appOnly)
        {
            try
            {
                // Get all current subscriptions
                var subscriptions = await graphClient.Subscriptions
                    .GetAsync();

                foreach (var subscription in subscriptions.Value)
                {
                    // Delete the subscription
                    await graphClient.Subscriptions[subscription.Id]
                        .DeleteAsync();

                }
            }
            catch (Exception ex)
            {
                var t = true;
                //logger.LogError(ex, "Error deleting existing subscriptions");
            }
        }


        GraphServiceClient GetAuthenticatedClient()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // The client credentials flow requires that you request the
            // /.default scope, and preconfigure your permissions on the
            // app registration in Azure. An administrator must grant consent
            // to those permissions beforehand.
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tennantId, appId, appSecret, options);

            return new GraphServiceClient(clientSecretCredential, scopes);
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

                ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Keep-Alive", "true");

                var requestBody = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", appId),
                    new KeyValuePair<string, string>("scope", scope),
                    new KeyValuePair<string, string>("client_secret", appSecret),
                    new KeyValuePair<string, string>("grant_type", grantType)
                };

                Uri path = new Uri(string.Format("{0}/{1}/{2}", baseAuthAddress, tennantId, authPath));

                var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(requestBody) };
                var httpResponse = await httpClient.SendAsync(request);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(serialisedContent);
                    this.authToken = jsonObject.access_token;

                }

            }

        }
    }
}
