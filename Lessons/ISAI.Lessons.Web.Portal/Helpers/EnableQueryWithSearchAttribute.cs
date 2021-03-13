using ISAI.Lessons.EntityFramework;
using Microsoft.AspNet.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;

namespace ISAI.Lessons.Web.Portal.Helpers
{


    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class EnableQueryWithSearchAttribute : EnableQueryAttribute
    {
        public Type ModelType { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var request = actionExecutedContext.Request;
            var query = request.GetQueryNameValuePairs().ToList();
            var searchParam = query.FirstOrDefault(q => q.Key == "$search");
            if (!String.IsNullOrWhiteSpace(searchParam.Key))
            {
                if (!String.IsNullOrWhiteSpace(searchParam.Value))
                {
                    string filterString = null;
                    var filter = query.FirstOrDefault(q => q.Key == "$filter");
                    if (!String.IsNullOrWhiteSpace(filter.Key))
                    {
                        filterString = filter.Value;
                    }

                    var odataSearchValue = searchParam.Value.Replace("'", "''");
                    var props = this.ModelType
                        .GetProperties()
                        .Where(pi => pi.GetCustomAttribute<AllowSearchAttribute>() != null || pi.PropertyType.Equals(typeof(string)))
                        .Select(pi => pi.Name)
                        .ToList();

                    if (props.Count > 0)
                    {
                        if (!String.IsNullOrWhiteSpace(filterString))
                        {
                            filterString = "(" + filterString + ") and ";
                        }

                        filterString += "(contains(" +
                                        String.Join(", '" + odataSearchValue + "') or contains(", props) + ", '" +
                                        odataSearchValue + "'))";
                    }

                    request.Properties[HttpPropertyKeys.RequestQueryNameValuePairsKey] =
                        query.Where(q => q.Key != "$search" && q.Key != "$filter").Concat(new[] { new KeyValuePair<string, string>("$filter", filterString) }).ToArray();
                }
                else
                {
                    request.Properties[HttpPropertyKeys.RequestQueryNameValuePairsKey] =
                        query.Where(q => q.Key != "$search").ToArray();
                }
            }

            
            base.OnActionExecuted(actionExecutedContext);
        }
    }

}