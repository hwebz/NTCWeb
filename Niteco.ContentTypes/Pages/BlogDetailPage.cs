using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.Extension;
using Niteco.ContentTypes.Statistics.Interfaces;
using Niteco.UI.Tags.EditorDescriptors;

namespace Niteco.ContentTypes.Pages
{
    [ContentType(DisplayName = "BlogDetailPage", GUID = "c51bb29b-b498-4b8d-990d-6c563103e7c7", Description = "Blog Detail Page", GroupName = GroupNames.Specialized)]
    [SiteImageUrl]
    public class BlogDetailPage : SitePageData, IHaveTag, IHaveAuthor, IHaveCategory
    {
        [Required]
        [Display(Name = "Title", GroupName = SystemTabNames.Content, Order = 10)]
        public virtual string Title { get; set; }

        [Required]
        [Display(Name = "Thumbnail", GroupName = GroupNames.Thumbnail, Order = 20)]
        public virtual ContentReference Thumbnail { get; set; }

        [Display(Name = "Blog category", GroupName = SystemTabNames.Content, Order = 20)]
        [UIHint("blog-category")]
        public virtual string BlogCategory { get; set; }

        [Display(Name = "Author", GroupName = SystemTabNames.Content, Order = 40)]
        [AllowedTypes(typeof(AuthorPage))]
        public virtual ContentReference Author { get; set; }

        [UIHint("Tags")]
        [NitecoTags(AllowSpaces = true, AllowDuplicates = true, CaseSensitive = false)]
        [Display(Order = 30)]
        public virtual string Tags { get; set; }

        [Display(GroupName = GroupNames.Thumbnail, Order = 90, Name = "Summary Description")]
        [UIHint(UIHint.Textarea)]
        public virtual string SummaryDes { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 80)]
        public virtual XhtmlString Content { get; set; }

        [ScaffoldColumn(false)]
        public override HtmlBlock MainContent { get; set; }
        [ScaffoldColumn(false)]
        public override string Heading { get; set; }

        [ScaffoldColumn(false)]
        public override XhtmlString ShortDescription
        {
            get { return new XhtmlString(Title); }
            set { value = new XhtmlString(Title); }
        }

        [ScaffoldColumn(false)]
        public override Url BackgroundImage { get; set; }
        [ScaffoldColumn(false)]
        public override Url MediumBackgroundUrl { get; set; }
        [ScaffoldColumn(false)]
        public override Url SmallBackgroundUrl { get; set; }
        public string AuthorName
        {
            get
            {
                var authorName = "Unknown";
                if (this.Author != null)
                {
                    AuthorPage authorPage;
                    var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
                    if (contentLoader.TryGet(this.Author, out authorPage))
                    {
                        if (!authorPage.IsDeleted)
                            authorName = authorPage.AuthorName;
                    }
                }
                return authorName;
            }
        }

        public BlogListingPage BlogListPage
        {
            get
            {
                BlogListingPage blogListPage;
                var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
                if (contentLoader.TryGet(this.ParentLink, out blogListPage))
                {
                    return blogListPage;
                }
                return null;
            }
        }

        public string SearchTextBlock
        {
            get { return Content.ToBlocksIncludedString(); }
            set { }
        }
    }
}