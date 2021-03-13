using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ISAI.Lessons.Web.Portal.Helpers
{
    public static class HMTLHelperExtensions
    {
        public static string IsSelected(this HtmlHelper html, string controller = null, string action = null, string cssClass = null)
        {

            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            if (String.IsNullOrEmpty(controller))
                controller = currentController;

            if (String.IsNullOrEmpty(action))
                action = currentAction;

            return controller.ToLower() == currentController.ToLower() && action.ToLower() == currentAction.ToLower() ?
                cssClass : String.Empty;
        }

        public static string IsSelected(this HtmlHelper html, string[] controllers = null, string action = null, string cssClass = null)
        {

            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            for (int i = 0; i < controllers.Length; i++)
            {

                var controller = controllers[i];


                if (String.IsNullOrEmpty(controller))
                    controller = currentController;

                if (String.IsNullOrEmpty(action))
                    action = currentAction;

                if (controller.ToLower() == currentController.ToLower() && action.ToLower() == currentAction.ToLower())
                {
                    return cssClass;
                }


            }

            return String.Empty;
        }

        public static string IsHeaderSelected(this HtmlHelper html, string controller)
        {

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            if (currentController.StartsWith(controller))
            {
                return "active";
            }

            return String.Empty;
        }



        public static string PageClass(this HtmlHelper html)
        {
            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            return currentAction;
        }


        public static MvcHtmlString DescriptionEnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

            var items =
                values.Select(
                   value =>
                   new SelectListItem
                   {
                       Text = GetEnumDescription(value),
                       Value = value.ToString(),
                       Selected = value.Equals(metadata.Model)
                   });
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return htmlHelper.DropDownListFor(expression, items, attributes);
        }

        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }



    }
}