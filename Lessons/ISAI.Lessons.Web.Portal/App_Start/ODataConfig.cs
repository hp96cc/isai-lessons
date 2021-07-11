using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Builder;
using ISAI.Lessons.EntityFramework.Models;

namespace ISAI.Lessons.Web.Portal.App_Start
{
    public class ODataConfig
    {
        public static void Register(HttpConfiguration config)
        {

            config.MapHttpAttributeRoutes();
            config.EnableDependencyInjection();

            // Web API configuration and services
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(10000);

            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<App>("Apps");
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<CustomerActivity>("CustomerActivities");
            builder.EntitySet<CustomerDevice>("CustomerDevices");
            builder.EntitySet<Lesson>("Lessons");
            builder.EntitySet<LessonGroup>("LessonGroups");
            builder.EntitySet<Subscription>("Subscriptions");
            builder.EntitySet<SubscriptionType>("SubscriptionTypes");

            config.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: builder.GetEdmModel());


            ODataModelBuilder builderReports = new ODataConventionModelBuilder();
            //builderReports.EntitySet<Reports.Models.OrderLineReport>("OrderLineReports");

            config.MapODataServiceRoute(
                routeName: "odatareports",
                routePrefix: "odatareports",
                model: builderReports.GetEdmModel());



        }
    }
}
