using ISAI.Lessons.EntityFramework;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.AzureVideo
{
    class Program
    {
        static readonly string baseAuthAddress = "https://login.microsoftonline.com/";
        static readonly string tennantId = "72b8f033-67c4-4524-964e-d3c01efa1acd";
        static readonly string authPath = "oauth2/v2.0/token";
        static readonly string appId = "88030882-6822-4b8d-b8ae-19fe43c38f89";
        static readonly string appSecret = "-u~-5YFpS0Q884b8~uW3Sc3oDSRy4gt7gg";
        static readonly string scope = "https://graph.microsoft.com/.default";
        static readonly string grantType = "client_credentials";

        static readonly string[] documentPermissons = new string[2] { "write", "read" };

        static readonly string videoPath = @"C:\Apps\Video\Original";

        static string authToken = "";

        private static HttpClient httpClient;

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                await AsyncMain(args);
            }).GetAwaiter().GetResult();
        }
        static async Task AsyncMain(string[] args)
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

                    //try
                    // {


                    var serialisedContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(serialisedContent);

                    System.Console.WriteLine(jsonObject.access_token.ToString());
                    authToken = jsonObject.access_token;

                    GraphServiceClient graphClient = GetAuthenticatedClient();

                    var message = new Message
                    {
                        Subject = "Meet for lunch?",
                        Body = new ItemBody
                        {
                            ContentType = BodyType.Text,
                            Content = "The new cafeteria is open."
                        },
                        ToRecipients = new List<Recipient>()
                        {
                            new Recipient
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = "craig@isai.co.uk"
                                }
                            }
                        },
                                            CcRecipients = new List<Recipient>()
                        {
                            new Recipient
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = "craig@isai.co.uk"
                                }
                            }
                        }
                    };

                    var saveToSentItems = true;

                    await graphClient.Users["craig.champion@scottishonlinelessons.com"]
                        .SendMail(message, saveToSentItems)
                        .Request()
                        .PostAsync();


                    return;


                    var uploadDriveItems = await graphClient.Drives["b!aVNtfIO6nkWwdDrL1WI8Vpoq8e1L-LNPvtx1zASO622kI24tnpiUT5WvCaNnZP4I"].Items["017W5IPYD74ZP36CAV7BAYCWK5HCICDXIH"].Children.Request().GetAsync();

                    foreach (var item in uploadDriveItems)
                    {
                        if (System.IO.File.Exists(Path.Combine(videoPath, item.Name)))
                        {
                            Console.WriteLine("Filename: {0} exists, skipping", item.Name);
                        }
                        else
                        {
                            await DownloadFileInChunks(graphClient, item);
                        }

                        //await graphClient.Drives["b!aVNtfIO6nkWwdDrL1WI8Vpoq8e1L-LNPvtx1zASO622kI24tnpiUT5WvCaNnZP4I"].Items[item.Id].Request().DeleteAsync();

                        //Check existence in database
                        using (var db = new LessonsDbContext())
                        {

                            var lesson = await db.Lesson.FirstOrDefaultAsync(x => x.PendingDownload == true && (x.SourceUrl == item.Name || x.SourceUrl == item.Name.Replace(".mp4", "")));

                            if (lesson != null)
                            {
                                Console.WriteLine("Match Lesson: {0} with file {1}", lesson.Name, item.Name);
                            }
                            else
                            {
                                Console.WriteLine("Could not Match Lesson:  file {0}", item.Name);
                                // await graphClient.Drives["b!aVNtfIO6nkWwdDrL1WI8Vpoq8e1L-LNPvtx1zASO622kI24tnpiUT5WvCaNnZP4I"].Items[item.Id].Request().DeleteAsync();

                            }

                        }

                    }


                    //} catch (Exception ex)
                    //{
                    //    var ts = true;
                    //}

                    System.Console.WriteLine("Complete");

                }

            }

            System.Console.ReadLine();

        }

        public static async Task DownloadFileInChunks(GraphServiceClient graphClient, DriveItem driveItem)
        {

            const long DefaultChunkSize = 1000 * 1024; // 50 KB, TODO: change chunk size to make it realistic for a large file.
            long ChunkSize = DefaultChunkSize;
            long offset = 0;         // cursor location for updating the Range header.
            byte[] bytesInStream;                    // bytes in range returned by chunk download.

            // Get the download URL. This URL is preauthenticated and has a short TTL.
            object downloadUrl;
            driveItem.AdditionalData.TryGetValue("@microsoft.graph.downloadUrl", out downloadUrl);

            // Get the number of bytes to download. calculate the number of chunks and determine
            // the last chunk size.
            long size = (long)driveItem.Size;
            int numberOfChunks = Convert.ToInt32(size / DefaultChunkSize);
            // We are incrementing the offset cursor after writing the response stream to a file after each chunk. 
            // Subtracting one since the size is 1 based, and the range is 0 base. There should be a better way to do
            // this but I haven't spent the time on that.
            int lastChunkSize = Convert.ToInt32(size % DefaultChunkSize) - numberOfChunks - 1;
            if (lastChunkSize > 0) { numberOfChunks++; }

            // Create a file stream to contain the downloaded file.
            using (FileStream fileStream = System.IO.File.Create(Path.Combine(videoPath, driveItem.Name)))
            {
            

                for (int i = 0; i < numberOfChunks; i++)
                {

                    Console.WriteLine("Downloading {0} - Chunk {1} of {2}", driveItem.Name, i, numberOfChunks);

                    // Setup the last chunk to request. This will be called at the end of this loop.
                    if (i == numberOfChunks - 1)
                    {
                        ChunkSize = lastChunkSize;
                    }

                    // Create the request message with the download URL and Range header.
                    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, (string)downloadUrl);
                    req.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(offset, ChunkSize + offset);

                    // We can use the the client library to send this although it does add an authentication cost.
                    // HttpResponseMessage response = await graphClient.HttpProvider.SendAsync(req);
                    // Since the download URL is preauthenticated, and we aren't deserializing objects, 
                    // we'd be better to make the request with HttpClient.
                    var client = new HttpClient();
                    HttpResponseMessage response = await client.SendAsync(req);

                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        bytesInStream = new byte[ChunkSize];
                        int read;
                        do
                        {
                            read = responseStream.Read(bytesInStream, 0, (int)bytesInStream.Length);
                            if (read > 0)
                                fileStream.Write(bytesInStream, 0, read);
                        }
                        while (read > 0);
                    }
                    offset += ChunkSize + 1; // Move the offset cursor to the next chunk.
                }
            }
            return;

        }
    

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public static GraphServiceClient GetAuthenticatedClient()
        {
            GraphServiceClient graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        string accessToken = authToken;

                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                        //// This header has been added to identify our sample in the Microsoft Graph service. If extracting this code for your project please remove.
                        //requestMessage.Headers.Add("SampleID", "aspnet-connect-sample");
                    }));
            return graphClient;
        }
    }
}
