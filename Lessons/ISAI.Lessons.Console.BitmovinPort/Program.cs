using ISAI.Lessons.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using Bitmovin.Api.Sdk;
using Bitmovin.Api.Sdk.Common.Logging;
using System.Threading.Tasks;
using Bitmovin.Api.Sdk.Models;
using System.Text;
using System.Linq;
using Bitmovin.Api.Sdk.Streams.Video;
using FFMpegCore;
using FFMpegCore.Enums;
using System.Net;
using System.Data.Entity.Infrastructure;

namespace ISAI.Lessons.Console.BitmovinPort
{
    internal class Program
    {

        //static string baseDirectory = @"F:\Video\";

        static string baseDirectory = @"F:\Video\";
        static string baseDirectoryOutput = @"C:\app\Output\";
        static string videoHostingUrl = "https://dev.scottishonlinelessons.com/";
       

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                await AsyncMain(args);
            }).GetAwaiter().GetResult();
        }

        static bool CheckUrlStatus(string Website)
        {
            try
            {
                var request = WebRequest.Create(Website) as HttpWebRequest;
                request.Method = "HEAD";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }
        }

        static async Task AsyncMain(string[] args)
        {

            var folders = Directory.GetDirectories(@"C:\Apps\Video\m3u8");
            StringBuilder sbQuery = new StringBuilder();

            for(int i=0; i < 1505; i++)
            {
                if (!Directory.Exists(@"C:\Apps\Video\m3u8" + "\\" + (i+1).ToString())) {
                    sbQuery.Append(" OR Id = " + (i + 1).ToString());
                }
            }

            System.Console.WriteLine(sbQuery.ToString());
            System.Console.ReadLine();
            return;

            using (var db = new LessonsDbContext())
            {
                //ffmpeg -i C:\app\BitMovin\input-100.mp4  -vcodec h264 -b:a 96k -start_number 0 -hls_time 10 -hls_list_size 0 -f hls -hls_enc 1 C:\app\BitMovin\100.m3u8
                var filesToProcess = Directory.GetFiles(baseDirectory);
                //var inputFile = Path.Combine(baseDirectory, "input-100.mp4");
                //var outputFile = Path.Combine(baseDirectoryOutput, "100.m3u8");

                StringBuilder sb = new StringBuilder();

                var tempDir = @"C:\Users\craig\Videos\";

                //foreach (var folder in Directory.GetDirectories(tempDir))
                //{
                //    if (folder.Contains("input-")) {

                //        var file = Directory.GetFiles(folder)[0];
                //        var fileId = Path.GetDirectoryName(file).Replace(tempDir, "");
                //        File.Copy(file, @"C:\app\Output\" + fileId + ".mp4");
                //        System.Console.Write(file);

                //    }

                //}

                //var lessons = db.Lesson.Where(x => x.Deleted == false).ToList();

                //foreach(var lesson in lessons)
                //{

                //    var m3usDir = @"C:\\Apps\\Video\\m3u8";

                //    var dirTotest = Path.Combine(m3usDir, lesson.Id.ToString());

                //    if(!Directory.Exists(dirTotest))
                //    {
                //        sb.AppendLine(lesson.Id.ToString());
                //    }


                //}


                //C:\Apps\Video\m3u8

                foreach (var file in filesToProcess)
                {
                    if (!file.EndsWith("mp4"))
                        continue;

                    //Check if the file has been processed
                    var fileName = Path.GetFileName(file);
                    var exportFileName = fileName.Replace("input-", "").Replace(".mp4", "");

                    var downloadUrl = string.Format("Invoke-WebRequest \"https://dev.scottishonlinelessons.com/mp4/{0}\" -OutFile c:\\videomp4\\{1}", fileName, fileName);
                    sb.AppendLine(downloadUrl);
                    //continue;

                    //var urlCheck = string.Format("https://dev.scottishonlinelessons.com/videotest/{0}/{1}.m3u8", exportFileName, exportFileName);
                    //if (CheckUrlStatus(urlCheck))
                    //{
                    //    System.Console.WriteLine("Processed, skipping - " + exportFileName);
                    //    continue;
                    //}


                    var createDirect = string.Format(" New-Item -Path {0}{1} -ItemType Directory", baseDirectoryOutput, exportFileName);
                    sb.AppendLine(createDirect);

                    //

                    var createDirectScreenShot = string.Format("C:\\ffmpeg\\bin\\ffmpeg.exe -i https://dev.scottishonlinelessons.com/mp4/{0} -ss 00:00:00 -frames:v 1 {1}{2}\\{3}.jpg", fileName, baseDirectoryOutput, exportFileName, exportFileName);
                    sb.AppendLine(createDirectScreenShot);

                    var command = string.Format("C:\\ffmpeg\\bin\\ffmpeg.exe -i https://dev.scottishonlinelessons.com/mp4/{0}  -vcodec h264 -b:a 96k -start_number 0 -hls_time 10 -hls_list_size 0 -f hls -hls_enc 1 {1}{2}\\{3}.m3u8", fileName, baseDirectoryOutput, exportFileName, exportFileName);
                    sb.AppendLine(command);
                }

                File.WriteAllText(@"c:\apps\command.txt", sb.ToString());
                System.Console.ReadLine();
                return;
                //var bitmovinApi = BitmovinApi.Builder
                //    .WithApiKey("6196124a-a52f-4a5c-929c-3af0030d6589")
                //    .WithLogger(new ConsoleLogger())
                //    .Build();



                //    var videos = await bitmovinApi.Streams.Video.ListAsync();

                //var pubVideos = videos.Items.Where(x => x.Status == StreamsVideoStatus.PUBLISHED);
                //var lessons = await db.Lesson.ToListAsync();

                //foreach (var video in videos.Items)
                //{
                //    var lesson = lessons.First(x => string.Format("{0} - {1}", x.Id, x.Name) == video.Title);

                //    if (lesson != null)
                //    {
                //        lesson.BitmovinId = video.Id;

                //        db.Entry(lesson).State = EntityState.Modified;
                //        await db.SaveChangesAsync();
                //    }    

                //}

                //return;
                //foreach (var fileToProcess in filesToProcess)
                //{
                //    var filePath = Path.Combine(baseDirectory, fileToProcess);
                //    var fileSize = new FileInfo(filePath).Length;

                //    if (fileSize > 0)
                //    {

                //        //TODO: check video does not alreayt exsit
                //        try
                //        {
                //            var stringFileId = fileToProcess.Replace(baseDirectory + "input-", "").Replace(".mp4", "").Trim();
                //            var lessonId = Convert.ToInt32(stringFileId);
                //            var lesson = await db.Lesson.FirstAsync(x => x.Id == lessonId && x.Deleted == false && x.BitmovinId == null);

                //            if (lesson != null)
                //            {
                //                System.Console.WriteLine(string.Format("Processing: {0} - {1}", lesson.Id, lesson.Name));
                //            }

                //            var downloadPath = string.Format("{0}input-{1}.mp4", videoHostingUrl, stringFileId);

                //            var response = await bitmovinApi.Streams.Video.CreateAsync(new StreamsVideoCreateRequest()
                //            {
                //                AssetUrl = downloadPath,
                //                Title = string.Format("{0} - {1}", lesson.Id, lesson.Name),
                //                Description = stringFileId,
                //                Signed = true,

                //                EncodingProfile = StreamsEncodingProfile.FIXED_RESOLUTIONS

                //            });

                //            bitmovinApi.Streams.

                //            lesson.BitmovinId = response.Id;
                //            lesson.DateModified = DateTime.UtcNow;

                //            db.Entry(lesson).State = EntityState.Modified;
                //            await db.SaveChangesAsync();

                //        }
                //        catch (Exception ex)
                //        {
                //            (string.Format("Error: {0} - {1}", ex.Message, ex.StackTrace));

                //        }


                //    }



                // }

            }

            System.Console.ReadLine();
        }
    }

}
