using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.WebPages;

namespace Niteco.Web.Capabilities
{
    public class DeviceDetector
    {
        private static readonly Regex MobilePattern = new Regex("IEMobile|Windows CE|iPod|NetFront|PlayStation|MIDP|UP\\.Browser|Symbian|Nintendo|Android|iPhone", RegexOptions.IgnoreCase);

        public const string DesktopUserAgent = "Mozilla/4.0 (compatible; MSIE 6.1; Windows XP)";

        public const string MobileUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows CE; IEMobile 8.12; MSIEMobile 6.0)";

        public static bool IsMobileAutoMode(string userAgent)
        {
            return MobilePattern.IsMatch(userAgent) && !IsTablet(userAgent);
        }

        public static bool IsMobileOverriddenMode(string userAgent)
        {
            return String.CompareOrdinal(userAgent, MobileUserAgent) == 0;
        }

        public static bool IsMobileOverriddenMode(HttpContextBase context)
        {
            return context.GetOverriddenBrowser().IsMobileDevice;
        }

        public static bool IsMobile(HttpContextBase context)
        {
            var userAgent = HttpUtility.UrlDecode(context.Request.UserAgent);
            if (string.IsNullOrEmpty(userAgent))
            {
                return false;
            }

            return (IsMobileAutoMode(userAgent) || IsMobileOverriddenMode(userAgent));
        }

        public static bool IsMobileOnDesktop(HttpContextBase context)
        {
            var userAgent = context.Request.UserAgent;
            if (string.IsNullOrEmpty(userAgent))
            {
                return false;
            }

            return !DeviceDetector.IsMobileAutoMode(userAgent) && DeviceDetector.IsMobileOverriddenMode(context);
        }

        public static bool IsTablet(string userAgent)
        {
            return userAgent.IndexOf("ipad", System.StringComparison.OrdinalIgnoreCase) > -1 ||
                   (userAgent.IndexOf("android", System.StringComparison.OrdinalIgnoreCase) > -1 &&
                    userAgent.IndexOf("mobile", System.StringComparison.OrdinalIgnoreCase) < 0);
        }

        public static bool IsTablet(HttpContextBase context)
        {
            var userAgent = context.Request.UserAgent;
            if (string.IsNullOrEmpty(userAgent))
            {
                return false;
            }

            return IsTablet(userAgent);
        }
    }
}