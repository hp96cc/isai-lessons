using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using ISAI.Lessons.Web.Portal.App_Start;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ISAI.Lessons.Web.Portal
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDA0NzQ2QDMxMzgyZTM0MmUzMGZCb2U1Ung1czRnMmVpUmsvKy9HNXE1dk9CRnU4TjJLa3hDQnZIKzlTalE9;NDA0NzQ3QDMxMzgyZTM0MmUzMGpBNG5BenlydEk5dE1uK3duTXlaQ0VCUkhyeXF6Z05NN29sYnFwdzRSRFk9;NDA0NzQ4QDMxMzgyZTM0MmUzME94eERzNEk0WDJDRkNpWlZqTUxTWEZSM3g0STVlKzZBdVk1VmZyKzFmeXc9;NDA0NzQ5QDMxMzgyZTM0MmUzMEVGdWJDTzdPSnpJaVFFeWJpbkQwT08wekpqMEQzWDQ2WW5McWg2ZlpTVlU9;NDA0NzUwQDMxMzgyZTM0MmUzMEw5MVh0UC9UQ1JJeVE0R1RFWVloNDhJZmtISWd6MlF5KzVzbGpheWpzaDQ9;NDA0NzUxQDMxMzgyZTM0MmUzMGx2TG94b05ER3QyMk9xNlVBUlZpVkFETzhUV1BnVGZBdnpLOUxaZ0JOU1U9;NDA0NzUyQDMxMzgyZTM0MmUzMFdUc2F5UG1mV3ozYi90TVdUekJUa1NmbkxQWHJzZytiT0NQdUQvMllRNkU9;NDA0NzUzQDMxMzgyZTM0MmUzMG1HamZpeHhqM1pTbENIaXZhMlNaUzhLRGM0bklVMWVxa0lmVmR4d0xmWUU9;NDA0NzU0QDMxMzgyZTM0MmUzMGdGSXZCUUk4WHFVc1BsUTZ0QlBLbDZObjVQeW83Zll1NFNxZ04zQ3RwUWc9;NDA0NzU1QDMxMzgyZTM0MmUzMEVacGYwNEd6emtGTUxObFB4bW9TYUxTUWFmWFpNdFFSYnJwbWlGZVBpY0U9");

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(config =>
            {
                ODataConfig.Register(config);
                WebApiConfig.Register(config);

            });
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            serializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

            var serializer = JsonSerializer.Create(serializerSettings);
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);
        }
    }
}
