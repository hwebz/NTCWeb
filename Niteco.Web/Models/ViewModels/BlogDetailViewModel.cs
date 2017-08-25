
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Models.ViewModels
{
    public class BlogDetailViewModel : PageViewModel<BlogDetailPage>
    {
        public BlogDetailViewModel(BlogDetailPage currentPage) : base(currentPage)
        {
        }

        public IEnumerable<BlogDetailPage> RelatedBlogsDetailPages { get; set; }
        public List<BlogCategory> ListBlogCategory { get; set; }
        public List<Authors> ListAuthors { get; set; }
        public List<Tags> ListTags { get; set; }
        public List<BlogDetailPage> ListBlogDetail { get; set; }
        public List<JobPage> ListJobPage { get; set; }
    }
}