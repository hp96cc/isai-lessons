using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Builder;
using ISAI.Lessons.EntityFramework.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using ISAI.Lessons.EntityFramework.Reports;

namespace ISAI.Lessons.Web.Portal.App_Start
{
    public class ODataConfig
    {
        public static void Register(HttpConfiguration config)
        {

            config.MapHttpAttributeRoutes();
            config.EnableDependencyInjection();

            // Web API configuration and services
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(100000);

            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<App>("Apps");
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<CustomerActivity>("CustomerActivities");
            builder.EntitySet<CustomerDevice>("CustomerDevices");
            builder.EntitySet<Lesson>("Lessons");
            builder.EntitySet<LessonGroup>("LessonGroups");
            builder.EntitySet<Subscription>("Subscriptions");
            builder.EntitySet<SubscriptionCode>("SubscriptionCodes");
            builder.EntitySet<SubscriptionType>("SubscriptionTypes");
            builder.EntitySet<User>("Users");
            builder.EntitySet<TutorialSubject>("TutorialSubjects");
            builder.EntitySet<IdentityRole>("Roles");
            builder.EntitySet<TutorialSubjectTutorUser>("TutorialSubjectTutorUsers");
            builder.EntitySet<GroupTutorial>("GroupTutorials");
            builder.EntitySet<Tutorial>("Tutorials");
            builder.EntitySet<GroupTutorialGroup>("GroupTutorialGroups");

            config.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: builder.GetEdmModel());


            ODataModelBuilder builderReports = new ODataConventionModelBuilder();
            builderReports.EntitySet<LessonActivityReport>("LessonActivityReports");

            config.MapODataServiceRoute(
                routeName: "odatareports",
                routePrefix: "odatareports",
                model: builderReports.GetEdmModel());



        }
    }
}
