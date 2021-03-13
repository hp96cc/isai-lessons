
using Microsoft.Owin;
using Owin;
using ISAI.Lessons.Web.Portal;

[assembly: OwinStartupAttribute(typeof(ISAI.Lessons.Web.Portal.Startup))]
namespace ISAI.Lessons.Web.Portal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
