using System.Collections.Generic;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Models.ViewModels
{
    public class NewsListingViewModel : PageViewModel<NewsListingPage>
    {
        public NewsListingViewModel(NewsListingPage currentPage) : base(currentPage)
        {
        }

        public IEnumerable<NewsDetailPage> NewsDetailPages { get; set; }
        public bool IsAjaxRequestLastPage { get; set; }
    }
}