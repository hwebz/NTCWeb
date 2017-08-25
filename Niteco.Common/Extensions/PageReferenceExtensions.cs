using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace Niteco.Common.Extensions
{
    public static class PageReferenceExtensions
    {
        public static string GetSimpleAddressIfExits(this PageReference pageLink)
        {
            if (PageReference.IsNullOrEmpty(pageLink))
            {
                return string.Empty;
            }

            var page = pageLink.GetPage();
            if (page == null)
            {
                return string.Empty;
            }
            var simpleaddress = page.GetPropertyValue("PageExternalURL");
            string pageUrl;
            if (!string.IsNullOrEmpty(simpleaddress))
            {
                pageUrl = string.Format("/{0}/", simpleaddress);
            }
            else
            {
                var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
                pageUrl = urlResolver.GetUrl(pageLink);
            }
            return pageUrl;
        }
        public static string GetSimpleAddressIfExits(this ContentReference contentLink)
        {
            var pageref = contentLink.ToPageReference();
            return pageref.GetSimpleAddressIfExits();
        }
    }
}
