using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web.Mvc;
using Lucene.Net.Search;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.ContentTypes.Extension;
using Niteco.ContentTypes.Pages;
using Niteco.Search;
using Niteco.Web.Models.ViewModels;
using WebMarkupMin.AspNet4.Mvc;

namespace Niteco.Web.Controllers
{
    public class BlogListingPageController : PageController<BlogListingPage>
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly IContentLoader _contentLoader;

        public BlogListingPageController(IContentLoader contentLoader, CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
            _contentLoader = contentLoader;
        }
        [MinifyHtml]
        public ActionResult Index(BlogListingPage currentPage, int page = 0)
        {
            //            var listBlogDetail = new List<BlogDetailPage>();
            var md = new BlogsListingViewModel(currentPage);
            var count = 0;
            var listBlogDetail = _contentLoader.GetChildren<BlogDetailPage>(currentPage.ContentLink).Where(x => DateTime.Compare(x.StartPublish, DateTime.Now) < 0 &&
                                                                                    DateTime.Compare(x.StopPublish, DateTime.Now) > 0).ToList();
            if (!string.IsNullOrEmpty(Request.Unvalidated.QueryString["q"]))
            {
                var listBlogSearch = GetBlogSearch(currentPage, Request.Unvalidated.QueryString["q"]);
                md.ListBlogDetail = listBlogSearch;
                count = listBlogSearch.Count;
            }
            else
            {
                var listBlogAfterFilter = listBlogDetail;
                if (!string.IsNullOrEmpty(Request.Unvalidated.QueryString["cat"]))
                {
                    listBlogAfterFilter = Request.Unvalidated.QueryString["cat"].ToString() == "Uncategorized" ? listBlogDetail.Where(x => string.IsNullOrEmpty(x.BlogCategory)).ToList() : listBlogDetail.Where(x => x.BlogCategory == Request.QueryString["cat"].ToString()).ToList();
                }

                if (!string.IsNullOrEmpty(Request.Unvalidated.QueryString["author"]))
                {
                    listBlogAfterFilter = listBlogDetail.Where(x => x.AuthorName == Request.Unvalidated.QueryString["author"].ToString()).ToList();
                }

                if (!string.IsNullOrEmpty(Request.Unvalidated.QueryString["tag"]))
                {
                    listBlogAfterFilter = listBlogDetail.Where(x => x.Tags != null && x.Tags.Contains(Request.QueryString["tag"].ToString())).ToList();
                }
                ViewBag.ItemsPerPage = currentPage.ItemsPerPage;
                md.ListBlogDetail = listBlogAfterFilter;
                count = listBlogAfterFilter.Count;
            }
            ViewBag.ItemsPerPage = currentPage.ItemsPerPage;

            md.IsAjaxRequestLastPage = (page + 1) * currentPage.ItemsPerPage >= count;
            md.ListBlogDetail = md.ListBlogDetail.Skip(page * currentPage.ItemsPerPage)
                                                     .Take(currentPage.ItemsPerPage).ToList();


            if (HttpContext.Request.IsAjaxRequest() && page > 0)
            {
                if (md.ListBlogDetail.Any())
                {
                    return PartialView("_BlogItem", md);
                }
                return Content("StringForEndingListingPage");

            }

            //            md.ListBlogDetail = listBlogAfterFilter;
            var listCategory = listBlogDetail.Select(x => x.BlogCategory).Distinct().ToList();
            //            var listCategory = CategoryHelper.GetCategories("Blog Category").Select(x => x.Name);
            var listBlogCategory = (from categoryName in listCategory
                                    let blogCount = listBlogDetail.Count(x => x.BlogCategory == categoryName)
                                    select new BlogCategory() { BlogCount = blogCount, Category = !string.IsNullOrEmpty(categoryName) ? categoryName : "Uncategorized" }).Where(x => x.BlogCount > 0)
                                                                                                                                                                 .OrderByDescending(x => x.BlogCount)
                                                                                                                                                                 .ToList();
            md.ListBlogCategory = listBlogCategory;

            var listTagCategory = new List<string>();
            // 1,2,3 | 3,2,4 | 3,4,5
            foreach (var tag in listBlogDetail.Select(x => x.Tags).ToList())
            {
                if (!string.IsNullOrEmpty(tag))
                    listTagCategory.AddRange(tag.Split(','));
            }
            listTagCategory = listTagCategory.Distinct().ToList();
            md.ListTags = (from tagName in listTagCategory let blogCount = listBlogDetail.Count(x => !string.IsNullOrEmpty(x.Tags) && x.Tags.Contains(tagName)) select new Models.ViewModels.Tags() { BlogCount = blogCount, TagName = tagName }).OrderByDescending(x => x.BlogCount).ToList();

            var listAuthor = listBlogDetail.Where(x => !x.IsDeleted).Select(x => x.AuthorName).Distinct().ToList();
            md.ListAuthors = (from authorName in listAuthor let blogCount = listBlogDetail.Count(x => x.AuthorName == authorName) select new Authors() { BlogCount = blogCount, AuthorName = authorName }).OrderByDescending(x => x.BlogCount).ToList();
            //            md.ListTags = listTags;
            var listJob = new List<JobPage>();
            if (currentPage.JobRoot != null)
            {
                CareerPage carrerPage;
                if (_contentLoader.TryGet(currentPage.JobRoot, out carrerPage))
                {
                    foreach (var contentAreaItem in carrerPage.JobOpenings.Items)
                    {
                        JobPage jobPage;
                        if (_contentLoader.TryGet(contentAreaItem.ContentLink, out jobPage))
                        {
                            listJob.Add(jobPage);
                        }
                    }
                }
            }
            //                listJob = _contentLoader.GetChildren<JobPage>(currentPage.JobRoot).OrderBy(x => x.Created).Take(3).ToList();
            md.ListJobPage = listJob.Take(3).ToList();
            return PartialView(md);
        }

        private List<BlogDetailPage> GetBlogSearch(BlogListingPage currentPage, string querySearch)
        {
            querySearch = querySearch == null ? string.Empty : querySearch.Trim().ToLower();

            // Paging
            var pageNumber = 1;
            var pageSize = 100;

            var sortFields = new Collection<SortField>();
            sortFields.Add(SortFieldFactory.CreateSortField(Field.Title, true));

            int totalItems;
            var pages = new List<BlogDetailPage>();

//            pages = CmsSearchHandler.Search<BlogDetailPage>(querySearch, (ContentReference)null, pageNumber, pageSize,
//                     sortFields, out totalItems).ToList();
//            if (pages.Count != 0) return pages;
            var listBlogDetail = _contentLoader.GetChildren<BlogDetailPage>(currentPage.ContentLink).Where(x => DateTime.Compare(x.StartPublish, DateTime.Now) < 0 &&
                                                                                   DateTime.Compare(x.StopPublish, DateTime.Now) > 0).ToList();
            pages = listBlogDetail.Where(x => (x.Title + "").ToLower().Contains(querySearch) || (x.SummaryDes + "").ToLower().Contains(querySearch) || (x.Content.ToBlocksIncludedString() + "").ToLower().Contains(querySearch)).ToList();
            return pages;
        }
    }


}