using System;
using System.Linq;
using System.Web.Http;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.WebApi;


namespace ISAI.Lessons.Web.Public.Controllers
{
    public class LessonUmbracoController : UmbracoApiController
    {

        [HttpGet]
        [Route("api/lessonumbraco/{id}")]
        public IHttpActionResult GetLessonUrlBySystemid(int id)
        {
            long totalRecords;

            var lesson = Services.ContentService.GetPagedOfType(1075, 0, 10000, out totalRecords, null)
                .FirstOrDefault(x => x.Properties["mediaId"] != null && x.Properties["mediaId"].GetValue() != null && x.Properties["mediaId"].GetValue().ToString().Equals(id.ToString()));

            if (lesson != null)
            {
                var url = Umbraco.Content(lesson.Id).Url(mode: UrlMode.Relative);
                Uri uri = new Uri(url, UriKind.Relative);
                return Redirect(uri);

            }

            return InternalServerError();

        }


        [HttpGet]
        [Route("api/lessonumbracotest/{id}")]
        public IHttpActionResult GetTestLessonUrlBySystemid(int id)
        {
            long totalRecords;

            var lesson = Services.ContentService.GetPagedOfType(1075, 0, 10000, out totalRecords, null)
                .FirstOrDefault(x => x.Properties["mediaId"] != null && x.Properties["mediaId"].GetValue() != null && x.Properties["mediaId"].GetValue().ToString().Equals(id.ToString()));

            if (lesson != null)
            {
                var url = Umbraco.Content(lesson.Id).Url(mode: UrlMode.Relative);
                Uri uri = new Uri(url +"?TestMode=true", UriKind.Relative);
                return Redirect(uri);

            }

            return InternalServerError();

        }


    }

}