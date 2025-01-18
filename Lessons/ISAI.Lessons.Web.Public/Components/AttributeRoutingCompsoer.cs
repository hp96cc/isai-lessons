using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace ISAI.Lessons.Web.Public.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ApiConfigurationComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Insert<ApiConfigurationComponent>();
        }
    }

    public class ApiConfigurationComponent : IComponent
    {
        public void Initialize()
        {
            RouteTable.Routes.MapMvcAttributeRoutes();
            GlobalConfiguration.Configuration.MapHttpAttributeRoutes();
            //GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    //ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    Formatting = Formatting.Indented
                }
            });


        }

        public void Terminate()
        {

        }

    }


}