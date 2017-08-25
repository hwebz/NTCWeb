using System;
using System.IO;
using EPiServer.ServiceLocation;

namespace Amaze.Web.ScheduledJobs
{
    [EPiServer.PlugIn.ScheduledPlugIn(DisplayName = "Sitemap Builder",
       Description = "Constructs a sitemap.xml file in the root of the site. Sitemaps tells Google and other search engines how to navigate the site.")]
    public class GenerateSiteMap
    {
        public static string Execute()
        {
            var sitemapPath = System.Web.Hosting.HostingEnvironment.MapPath("~/sitemap.xml");
            if (!String.IsNullOrWhiteSpace(sitemapPath))
            {
                if (File.Exists(sitemapPath))
                {
                    File.Delete(sitemapPath);
                }
                var builder = ServiceLocator.Current.GetInstance<SiteMapBuilder>();
                builder.BuildSiteMap().Save(sitemapPath);
            }
            else
            {
                throw new InvalidOperationException("Could not map site map path");
            }
            return string.Empty;
        }
    }
}