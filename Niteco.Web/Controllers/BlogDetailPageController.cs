using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Editor;
using EPiServer.Web.Mvc;
using Niteco.ContentTypes;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Helpers;
using Niteco.Web.Models.ViewModels;
using WebMarkupMin.AspNet4.Mvc;

namespace Niteco.Web.Controllers
{
    public class BlogDetailPageController : PageController<BlogDetailPage>
    {
        private readonly IContentLoader _contentLoader;

        public BlogDetailPageController(IContentLoader iContentLoader)
        {
            _contentLoader = iContentLoader;
        }

        [MinifyHtml]
        public ActionResult Index(BlogDetailPage currentPage)
        {
            ViewBag.SubscriptionText = SiteSettingsHandler.Instance.SiteSettings.SubscriptionText + "";
            ViewBag.SubscriptionDescription = SiteSettingsHandler.Instance.SiteSettings.SubscriptionDescription + "";
            ViewBag.MailChimpApiKey = SiteSettingsHandler.Instance.SiteSettings.MailChimpApiKey + "";
            ViewBag.MailChimpListId = SiteSettingsHandler.Instance.SiteSettings.MailChimpListId + "";
            ViewBag.SubscriptionUrlPost = SiteSettingsHandler.Instance.SiteSettings.SubcriptionUrlPost + "";

            var blogDetailViewModel = new BlogDetailViewModel(currentPage);
            var allBlogDetail = _contentLoader.GetChildren<BlogDetailPage>(currentPage.ParentLink)
                                              .Where(
                                                     x => DateTime.Compare(x.StartPublish, DateTime.Now) < 0 &&
                                                          DateTime.Compare(x.StopPublish, DateTime.Now) > 0)
                                              .OrderByDescending(x => x.StartPublish).ToList();
            var category = currentPage.BlogCategory;
            var author = currentPage.AuthorName;
            var listRelated = allBlogDetail.Where(x => x.BlogCategory == category && x != currentPage).ToList();
            if (listRelated.Count < 3)
            {
                if (!string.IsNullOrEmpty(currentPage.Tags))
                {
                    var tags = currentPage.Tags.Split(',').ToList();
                    foreach (var tag in tags)
                    {
                        listRelated.AddRange(allBlogDetail.Where(x => x.Tags == tag && !listRelated.Contains(x) && x != currentPage));
                    }
                }
            }
            if (listRelated.Count() < 3)
            {
                listRelated.AddRange(allBlogDetail.Where(x => x.AuthorName == author && !listRelated.Contains(x) && x != currentPage));
            }
            blogDetailViewModel.RelatedBlogsDetailPages = listRelated.Take(3).OrderByDescending(x=>x.StartPublish);

            var listCategory = allBlogDetail.Select(x => x.BlogCategory).Distinct().ToList();
//            var listCategory = CategoryHelper.GetCategories("Blog Category").Select(x => x.Name);
            var listBlogCategory = (from categoryName in listCategory let blogCount = allBlogDetail.Count(x => x.BlogCategory == categoryName) select new BlogCategory() { BlogCount = blogCount, Category = !string.IsNullOrEmpty(categoryName) ? categoryName : "Uncategorized" }).OrderByDescending(x => x.BlogCount).ToList();
            blogDetailViewModel.ListBlogCategory = listBlogCategory;

            var listTagCategory = new List<string>();
            // 1,2,3 | 3,2,4 | 3,4,5
            foreach (var tag in allBlogDetail.Select(x => x.Tags).ToList())
            {
                if (!string.IsNullOrEmpty(tag))
                    listTagCategory.AddRange(tag.Split(','));
            }
            listTagCategory = listTagCategory.Distinct().ToList();
            blogDetailViewModel.ListTags = (from tagName in listTagCategory let blogCount = allBlogDetail.Count(x => !string.IsNullOrEmpty(x.Tags) && x.Tags.Contains(tagName)) select new Models.ViewModels.Tags() { BlogCount = blogCount, TagName = tagName }).OrderByDescending(x => x.BlogCount).ToList();

            var listAuthor = allBlogDetail.Where(x=>!x.IsDeleted).Select(x => x.AuthorName).Distinct().ToList();
            blogDetailViewModel.ListAuthors = (from authorName in listAuthor let blogCount = allBlogDetail.Count(x => x.AuthorName == authorName) select new Authors() { BlogCount = blogCount, AuthorName = authorName }).OrderByDescending(x => x.BlogCount).ToList();
            //            md.ListTags = listTags;
            var listJob = new List<JobPage>();
            BlogListingPage parentPage;
            if (_contentLoader.TryGet(currentPage.ParentLink, out parentPage))
            {
                if (parentPage.JobRoot != null)
                {
                    CareerPage carrerPage;
                    if (_contentLoader.TryGet(parentPage.JobRoot, out carrerPage))
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
//                md.ListJobPage = listJob.Take(3).ToList();
//                if (parentPage.JobRoot != null)
//                    listJob = _contentLoader.GetChildren<JobPage>(parentPage.JobRoot).OrderBy(x => x.Created).Take(3).ToList();
                blogDetailViewModel.ListJobPage = listJob.Take(3).ToList();
            }

            return View(blogDetailViewModel);
        }

         protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            // don't throw 404 in edit mode
            if (!PageEditing.PageIsInEditMode)
            {
                if (PageContext.Page.StopPublish <= DateTime.Now)
                {
                    filterContext.Result = new HttpStatusCodeResult(404, "Not found");
                    return;
                }
            }

            base.OnAuthorization(filterContext);
        }
    }
}