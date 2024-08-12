using CryptoNet;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Web;
using ISAI.Lessons.Models.ViewModels;

namespace ISAI.Lessons.Web.Public
{
    public class VideoHandler : IHttpHandler
    {
        const string _aesKeyFile = @"c:\apps\videokey.aes";
        ICryptoNet _cryptoNeiKey;
        const string appUrlPrefix = "https://portal.scottishonlinelessons.com/";

        public VideoHandler()
        {
            _cryptoNeiKey = new CryptoNetAes(new FileInfo(_aesKeyFile));
        }

        public void ProcessRequest(HttpContext context)
        {
            var token = context.Request.QueryString["token"];
            var lessonId = context.Request.QueryString["lessonId"];
            var isApp = context.Request.QueryString["isApp"] == null ? false : Convert.ToBoolean(context.Request.QueryString["isApp"]);

            if (IsTokenValid(token, Convert.ToInt32(lessonId)))
            {
                var actionType = context.Request.QueryString["actionType"];
                var videoRoot = string.Format(@"C:\Apps\Video\m3u8\{0}\", lessonId);

                if (actionType == "m3u8")
                {
                    var m3u8FilePath = Path.Combine(videoRoot, string.Format("{0}.m3u8", lessonId));
                    var m3u8FileContents = Parsem3u8File(File.ReadAllText(m3u8FilePath), lessonId, token, isApp);

                    context.Response.ContentType = "application/x-mpegURL";
                    context.Response.Write(m3u8FileContents);
                    return;

                }
                if (actionType == "ts")
                {
                    var segmentNumber = context.Request.QueryString["segmentNumber"];
                    var tsFilePath = Path.Combine(videoRoot, string.Format("{0}.ts", segmentNumber, token));
                    var tsFileContents = File.ReadAllBytes(tsFilePath);

                    context.Response.ContentType = "video/vnd.dlna.mpeg-tts";
                    context.Response.BinaryWrite(tsFileContents);
                    return;

                }
                else if (actionType == "key")
                {

                    var keyFilePath = Path.Combine(videoRoot, string.Format("{0}.m3u8.key", lessonId));
                    var keyFileContents = File.ReadAllBytes(keyFilePath);

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