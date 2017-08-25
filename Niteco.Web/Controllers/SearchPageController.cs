using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Web;
using EPiServer.Search;
using EPiServer.ServiceLocation;
using Lucene.Net.Search;
using Niteco.Common.Helpers;
using Niteco.Common.Pagination;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.ContentTypes;
using Niteco.ContentTypes.Pages;
using Niteco.Search;
using Niteco.Web.Business;
using Niteco.Web.Extensions;
using Niteco.Web.Models.Shared;
using Niteco.Web.Models.ViewModels;
using EPiServer.Web;
using EPiServer.Web.Hosting;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using Niteco.ContentTypes.Extension;
using WebMarkupMin.AspNet4.Mvc;

namespace Niteco.Web.Controllers
{
    public class SearchPageController : PageControllerBase<SearchPage>
    {
        private UrlResolver urlResolver;
        private ISiteSettings settingPage = SiteSettingsHandler.Instance.SiteSettings;
        private IContentRepository contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        public SearchPageController(UrlResolver urlResolver)
        {
            this.urlResolver = urlResolver;
        }

        [MinifyHtml]
        [ValidateInput(false)]
        public ActionResult Index(SearchPage currentPage, string keyword, int? page)
        {
            keyword = keyword == null ? string.Empty : keyword.Trim();

            // Paging
            var pageNumber = 1;
            if (page.HasValue && page > 1)
            {
                pageNumber = page.Value;
            }

            var pageSize = currentPage.PageSize;
            if (pageSize <= 0)
            {
                pageSize = 20;
            }

            var sortFields = new Collection<SortField>();
            sortFields.Add(SortFieldFactory.CreateSortField(Field.Title, true));

            int totalItems;
            var pages = new List<SitePageData>();

            pages = CmsSearchHandler.Search<SitePageData>(keyword, (ContentReference)null, pageNumber, pageSize,
                     sortFields, out totalItems).ToList();
            var searchItems = new List<SearchItemModel>();
            foreach (var pageData in pages)
            {
                if (pageData != null)
                {
                    searchItems.Add(ConvertPageToSearchItem(pageData));
                }
                else
                {
                    totalItems--;
                }
            }
            var searchPageLink = SiteSettingsHandler.Instance.SiteSettings.SearchPageLink;
            var model = new SearchContentModel(currentPage)
            {
                Items = new PagedList<SearchItemModel>(searchItems, pageNumber - 1, pageSize, totalItems),
                TotalItems = totalItems,
                Keyword = keyword,
                SearchPageUrl = !ContentReference.IsNullOrEmpty(searchPageLink) ? urlResolver.GetUrl(searchPageLink) : string.Empty,
                Pagination = new SimplePagination()
                {
                    TotalRecord = totalItems,
                    PageSession = pageSize
                }

            };

            return View(model);
        }


        [ValidateInput(false)]
        public JsonResult GetSearchResult(string q, int? page)
        {
            q = q == null ? string.Empty : q.Trim();

            // Paging
            var pageNumber = 1;
            if (page.HasValue && page > 1)
            {
                pageNumber = page.Value;
            }

            var pageSize = 10;
            //if (pageSize <= 0)
            //{
            //    pageSize = 20;
            //}

            var sortFields = new Collection<SortField>();
//            sortFields.Add(SortFieldFactory.CreateSortField(Field.Default, true));
            sortFields.Add(SortFieldFactory.CreateSortField(Field.Id, false));
//            sortFields.Add(SortFieldFactory.CreateSortField(Field.Title, false));
            int totalItems;
            var pages = new List<SitePageData>();

            pages = CmsSearchHandler.Search<SitePageData>(q, (ContentReference)null, pageNumber, pageSize,
                     sortFields, out totalItems).ToList();
            pages = pages.OrderByDescending(x => (x.Heading + "").ToLower().Contains(q)).ToList();
            var listBlogDetail = CmsSearchHandler.Search<BlogDetailPage>(string.Empty, (ContentReference)null, 1, 100,
                     sortFields, out totalItems).ToList();
            q = q.ToLower();
            var blogPages = listBlogDetail.Where(x => (x.Title + "").ToLower().Contains(q) || (x.SummaryDes + "").ToLower().Contains(q) || (x.Content.ToBlocksIncludedString() + "").ToLower().Contains(q)).ToList();
            pages.AddRange(blogPages);
            pages = pages.Distinct().ToList();
            try
            {
                var resultsObj = pages.Select(x => new
                {
                    title = x.DisplayName,
                    description = x.ShortDescription != null ? x.ShortDescription.ToString().GetRawContentFromHtmlSource().GetShortContent(59) : string.Empty,
                    url = GetUrl(x)
                });
                var md = new
                {
                    data = resultsObj,
                    success = 1,
                    currentPage = pageNumber,
                    hasShowMore = totalItems > pageSize * pageNumber,
                    message = resultsObj.Count() == 0 ? "The search returns no results" : ""
                };
                return Json(md, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var md = new
                {
                    data = "error",
                    success = 1,
                    message = "Something went wrong!",
                    exception = ex.Message
                };
                return Json(md, JsonRequestBehavior.AllowGet);
            }

        }

        private string GetUrl(SitePageData sitePageData)
        {
            if (sitePageData is CaseStudyPage)
            {
                OurWorkPage ourWorkPage;
                if (contentRepository.TryGet(sitePageData.ParentLink, out ourWorkPage))
                {
                    return UrlResolver.Current.GetUrl(ourWorkPage);
                }
            }
            else if (sitePageData is ServicePage)
            {
                var serviceListLink = UrlResolver.Current.GetUrl(settingPage.ServiceListRoot); //get setting;
                return serviceListLink;
            }
            else
            {
                return UrlResolver.Current.GetUrl(sitePageData.ContentLink);
            }
            return string.Empty;
        }

        private SearchItemModel ConvertPageToSearchItem(SitePageData page)
        {

            return new SearchItemModel
            {
                Title = page.DisplayName,
                LinkUrl = urlResolver.GetUrl(page.ContentLink)
            };
        }
    }
}
