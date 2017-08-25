using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Niteco.ContentTypes.Pages;
using Niteco.Common.Extensions;

namespace Amaze.Web.ScheduledJobs
{
    internal class SiteMapBuilder
    {
        private const string Protocol = @"http://";
        private readonly XNamespace _namespace;
        private readonly IContentLoader _pageRepo;
        private readonly ILanguageBranchRepository _langRepo;
        private readonly UrlResolver _urlResolver;
        private readonly FilterContentForVisitor _filter;
        private readonly string _host;
        private const String TimeFormat = "yyyy-MM-ddThh:mm:sszzz";

        public SiteMapBuilder(IContentLoader pageRepo, ILanguageBranchRepository langRepo, UrlResolver urlResolver)
        {
            _pageRepo = pageRepo;
            _langRepo = langRepo;
            _urlResolver = urlResolver;
            _filter = new FilterContentForVisitor();
            _host = SiteDefinition.Current.SiteUrl.Host;
            _namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        }

      
        public XmlDocument BuildSiteMap()
        {
            var xDoc = new XmlDocument();

            XmlNode xNode = xDoc.CreateXmlDeclaration("1.0", "utf-8", null);

            xDoc.AppendChild(xNode);

            XmlNode xUrlSet = xDoc.CreateElement("urlset");
            XmlAttribute xXmlns = xDoc.CreateAttribute("xmlns");
            xXmlns.Value = "http://www.sitemaps.org/schemas/sitemap/0.9";

            xUrlSet.Attributes.Append(xXmlns);
            xDoc.AppendChild(xUrlSet);

            int iCounter = 0;
            //var query = SearchClient.Instance.Search<SitePageData>().Filter(x=>x.VisibleInMenu.Match(true)).Take(1000);
            //var pages = query.GetContentResult();

            IList<ContentReference> descendants = _pageRepo.GetDescendents(ContentReference.StartPage).ToList();
            descendants.Add(ContentReference.StartPage);

            foreach (var descendant in descendants)
            {
                SitePageData page;
                if (_pageRepo.TryGet<SitePageData>(descendant, out page) && page.VisibleInMenu)
                {
                    var friendlyUrl = page.GetExternalUrl();

                    XmlComment xComment = xDoc.CreateComment(string.Format("{0} ({1}) - {2}", page.PageName, page.PageLink.ID, page.LanguageBranch));

                    XmlNode xUrlNode = xDoc.CreateElement("url");
                    XmlNode xLoc = xDoc.CreateElement("loc");
                    XmlNode xUrl = xDoc.CreateTextNode(friendlyUrl);

                    xUrlNode.AppendChild(xComment);

                    xLoc.AppendChild(xUrl);
                    xUrlNode.AppendChild(xLoc);

                    XmlNode xLastMod = xDoc.CreateElement("lastmod");
                    xLastMod.AppendChild(xDoc.CreateTextNode(page.Saved.ToString(TimeFormat)));
                    xUrlNode.AppendChild(xLastMod);

                    XmlNode xChangeFrequency = xDoc.CreateElement("changefreq");
                    xChangeFrequency.AppendChild(xDoc.CreateTextNode("daily"));
                    xUrlNode.AppendChild(xChangeFrequency);

                    XmlNode xPriority = xDoc.CreateElement("priority");
                    xPriority.AppendChild(xDoc.CreateTextNode(GetPriority(friendlyUrl).ToString("0.0", CultureInfo.CreateSpecificCulture("en-GB"))));
                    xUrlNode.AppendChild(xPriority);

                    xUrlSet.AppendChild(xUrlNode);

                    iCounter++;
                }
            }
            return xDoc;
        }
        /// <summary>
        ///     Gets the priority.
        /// </summary>
        private double GetPriority(string url)
        {
            int depth = new Uri(url).Segments.Count() - 1;

            return Math.Max(1.0 - (depth / 10.0), 0.5);
        }

    }
}