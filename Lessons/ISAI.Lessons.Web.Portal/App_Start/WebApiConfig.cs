using ISAI.Lessons.Web.Portal.Controllers.Api;
using System.IO;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ISAI.Lessons.Web.Portal.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            // Web API routes
            config.EnableCors(new EnableCorsAttribute("*", headers: "*", methods: "*"));

            config.Routes.MapHttpRoute(
              name: "DefaultApi",
              routeTemplate: "api/{controller}/{id}",
              defaults: new { id = RouteParameter.Optional }

          );

          

          
        }
    }
}
