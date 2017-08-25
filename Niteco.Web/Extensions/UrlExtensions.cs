using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Niteco.Common;
using Niteco.Common.Consts;

namespace Niteco.Web.Extensions
{
    public static class UrlExtensions
    {
        public static IHtmlString ImageUrl(this UrlHelper urlHelper, ContentReference contentLink, string preset = null)
        {
            var context = urlHelper.RequestContext.HttpContext;
            var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
            var publicUrl = urlResolver.GetUrl(contentLink);
            if (string.IsNullOrEmpty(publicUrl))
            {
                return new MvcHtmlString("");
            }
            var sb = new StringBuilder(publicUrl);

            if (string.IsNullOrEmpty(preset))
            {
                if (context.IsMobile())
                {
                    preset = "480";
                }
                else if (context.IsTablet())
                {
                    preset = "640";
                }
                else
                {
                    preset = "960";
                }
            }

            if (!string.IsNullOrEmpty(preset))
            {
                sb.Append(publicUrl.Contains("?") ? "&" : "?");
                sb.AppendFormat("preset={0}", preset);
            }

            return new MvcHtmlString(sb.ToString());
        }
        public static IHtmlString ImageUrl(this UrlHelper urlHelper, Url imageUrl, string preset = null)
        {
            var context = urlHelper.RequestContext.HttpContext;
            var publicUrl = imageUrl.ToString();
            if (string.IsNullOrEmpty(publicUrl))
            {
                return new MvcHtmlString("");
            }
            var sb = new StringBuilder(publicUrl);

            if (string.IsNullOrEmpty(preset))
            {
                if (context.IsMobile())
                {
                    preset = "480";
                }
                else if (context.IsTablet())
                {
                    preset = "640";
                }
                else
                {
                    preset = "960";
                }
            }

            if (!string.IsNullOrEmpty(preset))
            {
                sb.Append(publicUrl.Contains("?") ? "&" : "?");
                sb.AppendFormat("preset={0}", preset);
            }

            return new MvcHtmlString(sb.ToString());
        }
    }
}