using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using EPiServer;
using EPiServer.Cms.Shell;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Niteco.Common.Helpers;
using ArgumentException = System.ArgumentException;

namespace Niteco.Common.Extensions
{
    public static class PageExtensions
    {
        /// <summary>
        /// Returns the full, public URL of a page.
        /// </summary>
        /// <param name="page">The page whose friendly URL should be returned.</param>
        /// <returns>The friendly URL including host of the page based on the current HTTP context.</returns>
        public static string GetExternalUrl(this PageData page)
        {
            if (page == null)
            {
                return string.Empty;
            }

            if (HttpContext.Current.Request.ApplicationPath == null)
            {
                return string.Empty;
            }

            var url = new UrlBuilder(page.LinkURL);
            if (!string.IsNullOrEmpty(url.Scheme))
            {
                return page.LinkURL;
            }

            Global.UrlRewriteProvider.ConvertToExternal(
                url,
                !PageReference.IsNullOrEmpty(page.PageLink) ? page.PageLink : null,
                Encoding.UTF8);

            return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath.TrimEnd(new[] { '/' }) + url.ToString();
        }
        public static PageData GetAncestorAtLevel(this PageData page, ContentReference rootPage, int offset,
            IContentLoader contentLoader)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }
            if (ContentReference.IsNullOrEmpty(rootPage))
            {
                throw new ArgumentException("rootPage");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Offset cannot be negative");
            }

            if (contentLoader == null)
            {
                contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            }

            var list = contentLoader.GetAncestors(page.ContentLink)
                .Reverse()
                .Select(x => x.ContentLink)
                .ToList();

            if (list.Count > 0 && list[0].CompareToIgnoreWorkID(rootPage))
            {
                if (list.Count == offset + 1)
                {
                    return page;
                }
                if (list.Count > offset + 1)
                {
                    return contentLoader.Get<PageData>(list[offset + 1]);
                }
            }
            return null;
        }

        public static string EpiserverPublicUrl(this IContent content)
        {
            return content.PublicUrl();
        }

        public static string ToPublicUrl(this PageData page, UrlResolver urlResolver = null, IContentLoader contentLoader = null)
        {
            if (page == null)
            {
                return string.Empty;
            }

            if (urlResolver == null)
            {
                urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
            }

            switch (page.LinkType)
            {
                case PageShortcutType.Normal:
                case PageShortcutType.FetchData:
                    return urlResolver.GetUrl(page.PageLink, page.LanguageBranch(), new VirtualPathArguments()
                    {
                        ContextMode = ContextMode.Default
                    });
                case PageShortcutType.Shortcut:
                    var shortcutProperty = page.Property["PageShortcutLink"] as PropertyPageReference;
                    if (shortcutProperty != null && !ContentReference.IsNullOrEmpty(shortcutProperty.PageLink))
                    {
                        var shortcutPage = shortcutProperty.PageLink.GetPage(contentLoader);
                        return shortcutPage.ToPublicUrl(urlResolver, contentLoader);
                    }
                    break;
            }

            return page.LinkURL;
        }

        public static PageData GetPage(this PageReference pageReference, IContentLoader contentLoader = null)
        {
            if (PageReference.IsNullOrEmpty(pageReference))
            {
                return null;
            }

            if (contentLoader == null)
            {
                contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            }

            return contentLoader.Get<PageData>(pageReference);
        }

        public static Type GetPropertyType<TPageData, TValue>(this TPageData page, Expression<Func<TPageData, TValue>> expression)
            where TPageData : PageData
        {
            var propertyData = page.GetPropertyData(expression);

            return propertyData != null ? propertyData.GetType() : null;
        }

        public static PropertyData GetPropertyData<TPageData, TValue>(this TPageData page, Expression<Func<TPageData, TValue>> expression)
            where TPageData : PageData
        {
            var name = PropertyName.GetInternal(expression);

            return page.Property[name];
        }

        public static PropertyData GetPropertyData<TPageData>(this TPageData page, PropertyInfo propertyInfo)
            where TPageData : PageData
        {
            var name = PropertyName.GetInternal(propertyInfo);

            return page.Property[name];
        }
        public static String GetFriendlyUrl(this PageData page, String language)
        {
            return ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(page.ContentLink, language);
        }
        public static bool IsPublished(this PageData page)
        {
            return CheckPublishedStatus(page, PagePublishedStatus.Published);
        }

        private static bool CheckPublishedStatus(this PageData page, PagePublishedStatus status)
        {
            var checkPoint = DateTime.Now;
            if (status != PagePublishedStatus.Ignore)
            {
                if (page.PendingPublish)
                {
                    return false;
                }
                if (page.Status != VersionStatus.Published)
                {
                    return false;
                }
                //TODO: update those line to meet with episerver 9.
                if ((status >= PagePublishedStatus.PublishedIgnoreStopPublish) && (page.StartPublish > checkPoint))
                {
                    return false;
                }
                if ((status >= PagePublishedStatus.Published) && (page.StopPublish < checkPoint))
                {
                    
                    return false;
                }
            }

            return true;
        }
    }
}
