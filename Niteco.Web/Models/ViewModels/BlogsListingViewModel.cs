using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.DataAbstraction;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Models.ViewModels
{
    public class BlogsListingViewModel : PageViewModel<BlogListingPage>
    {
        public BlogsListingViewModel(BlogListingPage currentPage) : base(currentPage)
        {
        }

        public List<BlogCategory> ListBlogCategory { get; set; }
        public List<Authors> ListAuthors { get; set; }
        public List<Tags> ListTags { get; set; }
        public List<BlogDetailPage> ListBlogDetail { get; set; }
        public List<JobPage> ListJobPage { get; set; }
        public bool IsAjaxRequestLastPage { get; set; }
}

    public class BlogCategory
    {
        public string Category { get; set; }
        public int BlogCount { get; set; }
    }

    public class Authors
    {
        public string AuthorName { get; set; }
        public int BlogCount { get; set; }
    }

    public class Tags
    {
        public string TagName { get; set; }
        public int BlogCount { get; set; }
    }
}