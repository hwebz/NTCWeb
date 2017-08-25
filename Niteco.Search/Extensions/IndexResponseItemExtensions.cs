using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Niteco.Common.Search;


namespace Niteco.Search.Extensions
{
    public static class IndexResponseItemExtensions
    {
        /// <summary>
        /// Gets the page data for the current search result
        /// </summary>
        /// <param name="result">The <see cref="IndexResponseItem"/> to operate on</param>
        /// <returns>A <see cref="PageData"/> for the result or null if the page cannot be found</returns>
        public static PageData GetPageData(this IndexResponseItem result)
        {
            var guid = new Guid(result.VirtualPathNodes.Last());
            var map = PermanentLinkMapStore.Find(guid) as PermanentContentLinkMap;

            if (map != null)
            {
                return DataFactory.Instance.Get<PageData>(map.ContentReference);
            }

            return null;
        }

        public static T GetPageData<T>(this IndexResponseItem item) where T : PageData
        {
            var guid = new Guid(item.VirtualPathNodes.Last());
            var map = PermanentLinkMapStore.Find(guid) as PermanentContentLinkMap;

            if (map != null)
            {
                return ServiceLocator.Current.GetInstance<IContentLoader>().Get<T>(map.ContentReference);
            }
            return null;
        }

        public static MediaData GetMedia(this IndexResponseItem result, IContentRepository contentRepository)
        {
            return contentRepository.Get<IContentMedia>(new ContentReference(result.Id)) as MediaData;
        }

        ///// <summary>
        ///// Gets the unified file for the current search result
        ///// </summary>
        ///// <param name="result">The <see cref="IndexResponseItem"/> to operate on</param>
        ///// <returns>A <see cref="UnifiedFile"/> for the result or null if the file cannot be found</returns>
        //public static UnifiedFile GetUnifiedFile(this IndexResponseItem result)
        //{
        //    string fileUrl = string.Empty;
        //    if (PermanentLinkMapStore.TryToMapped(result.Id, out fileUrl))
        //    {
        //        return GenericHostingEnvironment.Instance.VirtualPathProvider.GetFile(HttpUtility.UrlDecode(fileUrl)) as UnifiedFile;
        //    }

        //    return null;
        //}

        //public static CmsItemType GetItemType(this IndexResponseItem result)
        //{
        //    try
        //    {
        //        return (CmsItemType)Enum.Parse(typeof(CmsItemType), result.ItemType);
        //    }
        //    catch
        //    {
        //        return CmsItemType.Page;
        //    }
        //}

        //public static string GetLinkUrl(this IndexResponseItem result)
        //{
        //    switch (result.GetItemType())
        //    {
        //        case CmsItemType.Page:
        //            PageData page = result.GetPageData();
        //            return UriSupport.AddLanguageSelection(page.LinkURL, page.LanguageBranch);

        //        case CmsItemType.Resource:
        //            UnifiedFile file = result.GetUnifiedFile();
        //            return file.VirtualPath;
        //    }

        //    return String.Empty;
        //}

        //public static string GetPreviewText(this IndexResponseItem result, int length)
        //{
        //    if (!string.IsNullOrEmpty(result.DisplayText))
        //    {
        //        if (result.DisplayText.Length > length)
        //        {
        //            return result.DisplayText.Substring(0, length);
        //        }
        //        else
        //        {
        //            return result.DisplayText;
        //        }
        //    }

        //    switch (result.GetItemType())
        //    {
        //        case CmsItemType.Page:
        //            PageData page = result.GetPageData();
        //            return page.GetPreviewText(length);

        //    }

        //    return String.Empty;
        //}

        //public static string GetTitleText(this IndexResponseItem result)
        //{
        //    switch (result.GetItemType())
        //    {
        //        case CmsItemType.Page:
        //            PageData page = result.GetPageData();
        //            return page.PageName;

        //        case CmsItemType.Resource:
        //            UnifiedFile file = result.GetUnifiedFile();
        //            return file.Name;
        //    }

        //    return String.Empty;
        //}
    }
}
