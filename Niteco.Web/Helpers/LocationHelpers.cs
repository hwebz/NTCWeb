using System;
using System.Net;
using System.Web;
using EPiServer.Logging.Compatibility;
using EPiServer.Personalization;
using EPiServer.Personalization.Providers.MaxMind;

namespace Niteco.Web.Helpers
{
    public static class LocationHelpers
    {
        private static ILog log = LogManager.GetLogger(typeof (LocationHelpers));

        public static bool IsFromVietNam()
        {
            return "VN".Equals(GetCountryCodeByIp(), StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetCountryCodeByIp()
        {
            try
            {
                var prov = Geolocation.Provider as GeolocationProvider;
                var ipAdress = GetIpAddress();
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Guest IP adress: {0}", ipAdress));
                }
                IPAddress ip = IPAddress.Parse(ipAdress);
                IGeolocationResult result = prov.Lookup(ip);
                return result.CountryCode;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string GetIpAddress()
        {
            HttpContext context = HttpContext.Current;
            string ip = context.Request.ServerVariables["True-Client-IP"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }

            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }
    }
}