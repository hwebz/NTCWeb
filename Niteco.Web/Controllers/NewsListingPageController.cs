using System;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Models.ViewModels;
using WebMarkupMin.AspNet4.Mvc;

namespace Niteco.Web.Controllers
{
    public class NewsListingPageController : PageControllerBase<NewsListingPage>
    {
        private readonly IContentLoader _contentLoader;

        public NewsListingPageController(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        [MinifyHtml]
        public ActionResult Index(NewsListingPage currentPage, int page = 0)
        {
            var allChildren = _contentLoader.GetChildren<NewsDetailPage>(currentPage.ContentLink)
                                            .Where(
                                                   x =>
                                                       DateTime.Compare(x.StartPublish, DateTime.Now) < 0 &&
                                                       DateTime.Compare(x.StopPublish, DateTime.Now) > 0)
                                            .OrderByDescending(x => x.StartPublish).ToList();
            var count = allChildren.Count;
            var newDetailPages =
                    allChildren.Skip(page * currentPage.ItemsPerLoad)
                    .Take(currentPage.ItemsPerLoad).ToList();
            ViewBag.ItemsPerLoad = currentPage.ItemsPerLoad;
            var isAjaxRequestLastPage = (page + 1) * currentPage.ItemsPerLoad>= count;
            var md = new NewsListingViewModel(currentPage)
                     {
                         NewsDetailPages = newDetailPages,
                         IsAjaxRequestLastPage = isAjaxRequestLastPage
                     };
            if (HttpContext.Request.IsAjaxRequest() && page > 0)
            {
                if (newDetailPages.Any())
                {
                    return PartialView("_NewsItems", md);
                }
                return Content("StringForEndingListingPage");
            }
            return View(md);
        }
    }
}