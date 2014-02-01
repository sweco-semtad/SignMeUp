using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace UtmaningenReg.Helpers
{
    public static class Extensions
    {
        // Extension method for the Html calss
        public static MvcHtmlString ActionImage(this HtmlHelper html, string action, string controllerName, object routeValues, string imagePath, string alt)
        {
            var url = new UrlHelper(html.ViewContext.RequestContext);

            // build the <img> tag
            var imgBuilder = new TagBuilder("img");
            imgBuilder.MergeAttribute("src", url.Content(imagePath));
            imgBuilder.MergeAttribute("alt", alt);
            string imgHtml = imgBuilder.ToString(TagRenderMode.SelfClosing);

            // build the <a> tag
            var anchorBuilder = new TagBuilder("a");

            anchorBuilder.MergeAttribute("href", url.Action(action, controllerName, routeValues));
            anchorBuilder.InnerHtml = imgHtml; // include the <img> tag inside
            string anchorHtml = anchorBuilder.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(anchorHtml);
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new NullReferenceException("IEnumerable source is null.");
            }
            if (action == null)
            {
                throw new NullReferenceException("Action action is null.");
            }
            foreach (T element in source)
            {
                action(element);
            }

            return source;
        }

        public static string Deltagarnamn(this Deltagare deltagare)
        {
            return deltagare.Förnamn + " " + deltagare.Efternamn;
        }
    }
}