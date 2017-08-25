using System.Collections.Generic;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Models.ViewModels
{
    public class NewsDetailViewModel : PageViewModel<NewsDetailPage>
    {
        public NewsDetailViewModel(NewsDetailPage currentPage) : base(currentPage)
        {
        }

        public IEnumerable<NewsDetailPage> RelatedNewsDetailPages { get; set; }
    }
}