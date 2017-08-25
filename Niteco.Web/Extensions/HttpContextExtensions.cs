using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.WebPages;
using Niteco.Web.Capabilities;

namespace Niteco.Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static readonly Regex MobilePatten = new Regex("IEMobile|Windows CE|iP(od|hone)|NetFront|PlayStation|MIDP|UP\\.Browser|Symbian|Nintendo|Android|iPhone", RegexOptions.IgnoreCase);

        public static bool IsMobile(this HttpContextBase httpContext)
        {
            if ((httpContext.GetOverriddenBrowser().IsMobileDevice && !httpContext.IsTablet())
                //|| DeviceDetector.IsMobileOnDesktop(httpContext)
                //|| DeviceDetector.IsMobileOverriddenMode(httpContext)
                )
            {
                return true;
            }

            return false;
        }

        public static bool IsMobileOnDesktop(this HttpContextBase httpContext)
        {
            return DeviceDetector.IsMobileOnDesktop(httpContext);
        }

        public static bool IsTablet(this HttpContextBase httpContext)
        {
            return DeviceDetector.IsTablet(httpContext);
        }
    }
}