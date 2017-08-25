using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Framework.Web;
using EPiServer.ServiceLocation;

namespace Niteco.Common.Extensions
{
    /// <summary>
    /// Extension methods for content
    /// </summary>
    public static class ContentExtensions
    {
        /// <summary>
        /// Shorthand for DataFactory.Instance.Get
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="contentLink"></param>
        /// <returns></returns>
        public static IContent Get<TContent>(this ContentReference contentLink) where TContent : IContent
        {
            return DataFactory.Instance.Get<TContent>(contentLink);
        }

        /// <summary>
        /// Filters content which should not be visible to the user. 
        /// </summary>
        public static IEnumerable<T> FilterForDisplay<T>(this IEnumerable<T> contents, bool requirePageTemplate = false, bool requireVisibleInMenu = false)
            where T : IContent
        {
            var accessFilter = new FilterAccess();
            var publishedFilter = new FilterPublished();
            contents = contents.Where(x => !publishedFilter.ShouldFilter((IContent) x) && !accessFilter.ShouldFilter((IContent) x));
            if (requirePageTemplate)
            {
                var templateFilter = ServiceLocator.Current.GetInstance<FilterTemplate>();
                templateFilter.TemplateTypeCategories = TemplateTypeCategories.Page;
                contents = contents.Where(x => !templateFilter.ShouldFilter((IContent) x));
            }
            if (requireVisibleInMenu)
            {
                contents = contents.Where(x => VisibleInMenu(x));
            }
            return contents;
        }

        private static bool VisibleInMenu(IContent content)
        {
            var page = content as PageData;
            if (page == null)
            {
                return true;
            }
            return page.VisibleInMenu;
        }

        public static IContent GetContent(this ContentReference contentReference, IContentLoader contentLoader = null)
        {
            if (ContentReference.IsNullOrEmpty(contentReference))
            {
                return null;
            }

            if (contentLoader == null)
            {
                contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            }

            return contentLoader.Get<IContent>(contentReference);
        }
       
    }
}
