using ISAI.Lessons.Core.Services;
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


            try
            {

                //var azureMediaService = new AzureMediaService();
                //string jobId = Guid.NewGuid().ToString();
                //var urls = await azureMediaService.EncodeFile(jobId, @"c:\temp\test.mp4", @"c:\temp\");

                //var t = true;
                //foreach (var url in urls)
                //{
                //    Console.WriteLine(url);
                //}


                //Console.WriteLine("Done. Copy and paste the Streaming URL ending in '/manifest' into the Azure Media Player at 'http://aka.ms/azuremediaplayer'.");
                //Console.WriteLine("See the documentation on Dynamic Packaging for additional format support, including CMAF.");
                //Console.WriteLine("https://docs.microsoft.com/azure/media-services/latest/dynamic-packaging-overview");


                var graphApi = new MicrosoftGraphApiService();

                var uploadDriveItems = await graphApi.GetFilesForFolder("b!aVNtfIO6nkWwdDrL1WI8Vpoq8e1L-LNPvtx1zASO622kI24tnpiUT5WvCaNnZP4I", "017W5IPYD74ZP36CAV7BAYCWK5HCICDXIH");


                foreach (var item in uploadDriveItems)
                {
                    var filePath = Path.Combine(videoPath, item.Name);

                    if (System.IO.File.Exists(filePath))
                    {
                        Console.WriteLine("Filename: {0} exists, skipping", item.Name);
                    }
                    else
                    {
                        await graphApi.DownloadFileInChunks(item, videoPath);
                    }

                    //await graphApi.DeleteFileAsync("b!aVNtfIO6nkWwdDrL1WI8Vpoq8e1L-LNPvtx1zASO622kI24tnpiUT5WvCaNnZP4I", item.Id);

                    //Check existence in database
                    using (var db = new LessonsDbContext())
                    {

                        var lesson = await db.Lesson.FirstOrDefaultAsync(x => x.PendingDownload == true && (x.SourceUrl == item.Name || x.SourceUrl == item.Name.Replace(".mp4", "")));

                        if (lesson != null)
                        {
                            Console.WriteLine("Match Lesson (Encodiing): {0} with file {1}", lesson.Name, item.Name);

                            var azureMediaService = new AzureMediaService();
                            //string jobId = Guid.NewGuid().ToString();
                            var encodingOutput = await azureMediaService.EncodeFile(lesson.Id.ToString(), filePath, @"c:\temp\");

                            if (encodingOutput != null)
                            {
                                lesson.PendingDownload = false;
                                lesson.AssetId = JsonConvert.SerializeObject(encodingOutput);
                                lesson.DateModified = DateTime.UtcNow;
                                db.Entry(lesson).State = EntityState.Modified;

                                await db.SaveChangesAsync();
                            }

                            await graphApi.DeleteFileAsync("b!aVNtfIO6nkWwdDrL1WI8Vpoq8e1L-LNPvtx1zASO622kI24tnpiUT5WvCaNnZP4I", item.Id);
                            System.IO.File.Delete(filePath);

                        }
                        else
                        {
                            Console.WriteLine("Could not Match Lesson:  file {0}", item.Name);
                            // await graphApi.DeleteFileAsync("b!aVNtfIO6nkWwdDrL1WI8Vpoq8e1L-LNPvtx1zASO622kI24tnpiUT5WvCaNnZP4I", item.Id);

                        }

                    }

                }


            }
            catch (Exception ex)
            {
                System.Console.Write(ex.Message);
                System.Console.Write(ex.StackTrace);
            }

            System.Console.WriteLine("Complete");
            System.Console.ReadLine();

        }

    }
}
