using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [ContentType(DisplayName = "BlogListingPage", GUID = "757e3a62-beec-4a6b-983d-d359abed724f", Description = "Blog Listing Page", GroupName = GroupNames.Specialized)]
    [SiteImageUrl]
    [AvailableContentTypes(
        Availability.Specific,
        Include = new[] { typeof(BlogDetailPage) })] // ...and underneath those we can't create additional start pages
    public class BlogListingPage : SitePageData
    {
        [Display(Name = "Number of items per page", GroupName = SystemTabNames.Content)]
        [Range(1, int.MaxValue)]
        public virtual int ItemsPerPage { get; set; }

        [AllowedTypes(typeof(CareerPage))]
        [Display(Order = 10)]
        public virtual ContentReference JobRoot { get; set; }

        [ScaffoldColumn(false)]
        public override HtmlBlock MainContent { get; set; }

        [Display(Name = "Loading Image", GroupName = SystemTabNames.Content)]
        [UIHint(UIHint.Image)]
        public virtual ContentReference LoadingImage { get; set; }

        [Display(Name = "Loading Text", GroupName = SystemTabNames.Content)]
        public virtual string LoadingText { get; set; }

        [Display(Name = "Search Title", GroupName = GroupNames.RightColumnnTitle, Order = 1)]
        public virtual string SearchTitle { get; set; }
        [Display(Name = "Filter Category Title", GroupName = GroupNames.RightColumnnTitle, Order = 2)]
        public virtual string FilterCategoryTitle { get; set; }
        [Display(Name = "Top Author Title", GroupName = GroupNames.RightColumnnTitle, Order = 3)]
        public virtual string TopAuthorTitle { get; set; }
        [Display(Name = "Top Tag Title", GroupName = GroupNames.RightColumnnTitle, Order = 4)]
        public virtual string TopTagTitle { get; set; }
        [Display(Name = "We Are Hiring Title", GroupName = GroupNames.RightColumnnTitle, Order = 5)]
        public virtual string WeAreHiringTitle { get; set; }

        [Display(Name = "We Are Hiring Description", GroupName = GroupNames.RightColumnnTitle, Order = 6)]
        [UIHint(UIHint.Textarea)]
        public virtual string WeAreHiringDescription { get; set; }
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            ItemsPerPage = 5;
        }
    }
}