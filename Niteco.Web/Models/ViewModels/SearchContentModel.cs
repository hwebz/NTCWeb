using System.Collections.Generic;
using System.Web;
using Niteco.Common.Pagination;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Models.Shared;

namespace Niteco.Web.Models.ViewModels
{
    public class SearchContentModel : PageViewModel<SearchPage>
    {
        public SearchContentModel(SearchPage currentPage)
            : base(currentPage)
        {
        }

        public string Title { get; set; }

        public string Keyword { get; set; }

        public string Tags { get; set; }
        public PagedList<SearchItemModel> Items { get; set; }

        public int TotalItems { get; set; }

        public string SearchPageUrl { get; set; }

        public SimplePagination Pagination { get; set; }
    }

    public class SearchItemModel
    {
        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public string LinkUrl { get; set; }
    }
}
