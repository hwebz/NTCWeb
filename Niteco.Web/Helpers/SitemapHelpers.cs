using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Web;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Niteco.Common.Extensions;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Helpers
{
    public class SitemapHelpers
    {
        static IContentLoader _contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        public static SitemapItem Root
        {
            get
            {
                var itm = new SitemapItem();
                var pageRouteHelper = ServiceLocator.Current.GetInstance<PageRouteHelper>();
                var currentPage = pageRouteHelper.Page as SitemapPage;
                var sitemapRoot = !ContentReference.IsNullOrEmpty(currentPage.SitemapRoot)
                    ? currentPage.SitemapRoot
                    : ContentReference.StartPage;
                var SitemapRootPage = _contentLoader.Get<PageData>(sitemapRoot);

                itm.CmsPageData = SitemapRootPage;
                itm.SitemapNode = null;
                itm.Title = SitemapRootPage.Name;
                itm.Indent = 0;
                itm.Url = SitemapRootPage.ToPublicUrl();
                itm.CallBackToGetChildren = GetChildrendByContext;
                return itm;
            }
        }

        public static IEnumerable<SitemapItem> GetChildrendByContext(SitemapItem item)
        {
            if (item.CmsPageData == null && item.SitemapNode == null)
            {
                yield break;
            }

            const int limitedIndent = 100; //SettingsHandler.Instance.SiteSettings.MaxLevelForMainMenu;

            if (item.Indent >= limitedIndent)
            {
                yield break;
            }

            var sitemapRootPage = item.CmsPageData;

            if (item.SitemapNode != null || sitemapRootPage != null)
            {
                IEnumerable<PageData> nodeChildrent = null;
                if (item.SitemapNode != null)
                {
                    nodeChildrent = _contentLoader.GetChildren<PageData>(item.SitemapNode.PageLink);
                }
                else
                {
                    if (sitemapRootPage != null)
                    {
                        nodeChildrent = _contentLoader.GetChildren<PageData>(sitemapRootPage.PageLink);
                    }
                }
                if (nodeChildrent != null)
                {
                    foreach (PageData nc in nodeChildrent)
                    {
                        var nodeContentData = nc as IContentData;
                        if (nodeContentData != null)
                        {
                            if (ServiceLocator.Current.GetInstance<TemplateResolver>()
                                .HasTemplate(nodeContentData, TemplateTypeCategories.Page))
                            {
                                yield return new SitemapItem()
                                {
                                    SitemapNode = nc,
                                    Title = nc.Name,
                                    Url = GetNodeContentPublicUrl(nc),
                                    Indent = item.Indent + 1,
                                    CallBackToGetChildren = GetChildrendByContext
                                };
                            }
                        }
                    }
                }
            }
        }

        public static string GetNodeContentPublicUrl(PageData node)
        {
            var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();

            if (node == null)
                return (string)null;
            return urlResolver.GetUrl(node.ContentLink, LanguageBranch(node), new VirtualPathArguments()
            {
                ContextMode = ContextMode.Default
            }) ?? string.Empty;
        }

        public static string LanguageBranch(IContent content)
        {
            var localizable = content as ILocalizable;
            if (localizable != null && localizable.Language != null)
            { return localizable.Language.Name; }
            else
            { return (string)null; }
        }

    }

    public class SitemapItem
    {
        public Func<SitemapItem, IEnumerable<SitemapItem>> CallBackToGetChildren;
        public PageData CmsPageData { get; set; }
        public PageData SitemapNode { get; set; }
        public int Indent { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public List<SitemapItem> Children
        {
            get
            {
                if (CallBackToGetChildren != null) return CallBackToGetChildren(this).ToList();
                return new List<SitemapItem>();
            }
        }
    }
}