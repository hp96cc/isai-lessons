using CryptoNet;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Web;
using ISAI.Lessons.Models.ViewModels;
using System.Configuration;
using System.Runtime.Remoting.Contexts;

namespace ISAI.Lessons.Web.Public
{
    public class VideoHandler : IHttpHandler
    {
        ICryptoNet _cryptoNeiKey;
        private readonly string _aesKeyFile = ConfigurationManager.AppSettings["Video.AesKeyFile"];
        private readonly string appUrlPrefix = ConfigurationManager.AppSettings["ISAI.Lessons.Web.ReturnUrl"];
        private readonly string _videoRootFolder = ConfigurationManager.AppSettings["ISAI.Lessons.VideoRootFolder"];
        private readonly string _videoDownloadFolder = ConfigurationManager.AppSettings["ISAI.Lessons.VideoDownloadFolder"];
    

        public VideoHandler()
        {
            _cryptoNeiKey = new CryptoNetAes(new FileInfo(_aesKeyFile));
        }

        public void ProcessRequest(HttpContext context)
        {
            //TODO: create caching for all files expact tokens
            //HttpCachePolicy cachePolicy = context.Response.Cache;
            //TimeSpan freshness = new TimeSpan(1, 0, 0, 0);
            //DateTime now = DateTime.Now;
            //cachePolicy.SetCacheability(HttpCacheability.Public);
            //cachePolicy.SetExpires(now.Add(freshness));
            //cachePolicy.SetMaxAge(freshness);
            //cachePolicy.SetValidUntilExpires(true);
            //cachePolicy.VaryByParams["id"] = true;

            if (context.Request.QueryString["lessonThumbId"] != null)
            {
                int lessonThumbId = Convert.ToInt32(context.Request.QueryString["lessonThumbId"]);
                var thumbFileName = string.Format(@"{0}{1}\{2}.jpg", _videoRootFolder, lessonThumbId, lessonThumbId);

                if (File.Exists(thumbFileName))
                {
                    //TODO: send back default image
                    context.Response.ContentType = "image/jpeg";
                    context.Response.BinaryWrite(new byte[0]);
                } 
                else
                {
                    var thumbBytes = File.ReadAllBytes(thumbFileName);
                    context.Response.ContentType = "image/jpeg";
                    context.Response.BinaryWrite(thumbBytes);
                }

                return;

            }

            var token = context.Request.QueryString["token"];
            var lessonId = context.Request.QueryString["lessonId"];
            var isApp = context.Request.QueryString["isApp"] == null ? false : Convert.ToBoolean(context.Request.QueryString["isApp"]);
           
            if (IsTokenValid(token, Convert.ToInt32(lessonId)))
            {
                var actionType = context.Request.QueryString["actionType"];
                var videoRoot = string.Format(@"{0}{1}\", _videoRootFolder, lessonId);

                if (actionType == "download")
                {
                    var zipFilePath = Path.Combine(_videoDownloadFolder, string.Format("{0}/{1}.zip", lessonId, lessonId));
                    var zipFilePathContents = Parsem3u8File(File.ReadAllText(zipFilePath), lessonId, token, isApp);

                    context.Response.BufferOutput = true;
                    context.Response.ContentType = "application/zip";
                    context.Response.TransmitFile(zipFilePath);
                    return;

                }
                else if (actionType == "m3u8")
                {
                    var m3u8FilePath = Path.Combine(videoRoot, string.Format("{0}.m3u8", lessonId));
                    var m3u8FileContents = Parsem3u8File(File.ReadAllText(m3u8FilePath), lessonId, token, isApp);

                    context.Response.BufferOutput = true;
                    context.Response.ContentType = "application/x-mpegURL";
                    context.Response.Write(m3u8FileContents);
                    return;

                }
                if (actionType == "ts")
                {
                    var segmentNumber = context.Request.QueryString["segmentNumber"];
                    var tsFilePath = Path.Combine(videoRoot, string.Format("{0}.ts", segmentNumber, token));
                    //var tsFileContents = File.ReadAllBytes(tsFilePath);

                    context.Response.BufferOutput = true;
                    context.Response.ContentType = "video/vnd.dlna.mpeg-tts";
                    context.Response.TransmitFile(tsFilePath);
                    //context.Response.BinaryWrite(tsFileContents);
                    return;

                }
                else if (actionType == "key")
                {

                    var keyFilePath = Path.Combine(videoRoot, string.Format("{0}.m3u8.key", lessonId));
                    var keyFileContents = File.ReadAllBytes(keyFilePath);

                    context.Response.BufferOutput = true;
                    context.Response.ContentType = "application/x-binary";
                    context.Response.BinaryWrite(keyFileContents);

                    return;

                }

            }

            throw new Exception();

        }


        private string Parsem3u8File(string fileContents, string lineNumber, string token, bool isApp)
        {

            var m3u8FileStringBuilder = new StringBuilder();

            using (var reader = new StringReader(fileContents))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {

                    if (line.Contains("m3u8.key"))
                    {
                        var handlerLine = string.Format("#EXT-X-KEY:METHOD=AES-128,URI=\"{0}/VideoHandler.ashx?actionType=key&lessonId={1}&token={2}\",IV=0x00000000000000000000000000000000", isApp ? appUrlPrefix : string.Empty, lineNumber, token);
                        m3u8FileStringBuilder.AppendLine(handlerLine);
                    }
                    else if (line.StartsWith(lineNumber))
                    {
                        var segmentNumber = Path.GetFileNameWithoutExtension(line);
                        var handlerLine = string.Format("{0}/VideoHandler.ashx?actionType=ts&lessonId={1}&segmentNumber={2}&token={3}", isApp ? appUrlPrefix : string.Empty, lineNumber, segmentNumber, token);
                        m3u8FileStringBuilder.AppendLine(handlerLine);
                    }
                    else
                    {
                        m3u8FileStringBuilder.AppendLine(line);
                    }
                }

                return m3u8FileStringBuilder.ToString();

            }
        }

        private bool IsTokenValid(string token, int lessonId)
        {
            try
            {
                byte[] data = HttpServerUtility.UrlTokenDecode(token);
                var decrypt = _cryptoNeiKey.DecryptToString(data);
                var lessonStreamingToken = JsonConvert.DeserializeObject<LessonStreamingToken>(decrypt);
                if(lessonStreamingToken.ExpiryDate > DateTime.UtcNow && lessonStreamingToken.LessonId == lessonId) { return true; }

            } catch(Exception ex)
            {
                throw ex;
            }

            return false;
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}