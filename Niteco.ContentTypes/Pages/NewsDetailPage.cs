using EPiServer.Core;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [ContentType(DisplayName = "NewsDetailPage", GUID = "6a4e281b-8c53-45f3-b1e6-0f519b1b6dee", Description = "")]
    public class NewsDetailPage : SitePageData
    {
        [Required]
        [Display(GroupName = SystemTabNames.Content, Order = 65, Name = "News Title")]
        public virtual string Title { get; set; }

        //[Display(GroupName = GroupNames.NewsContent, Order = 66, Name = "Main Teaser")]
        //public virtual XhtmlString MainTeaser { get; set; }

        //[Display(GroupName = GroupNames.NewsContent, Order = 70, Name = "Quote Block Content Area")]
        //[AllowedTypes(typeof(NewsQuoteBlock))]
        //public virtual ContentArea QuoteContentArea { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 80)]
        public virtual XhtmlString Content { get; set; }

        [Display(GroupName = GroupNames.Thumbnail, Order = 90, Name = "Summary Description")]
        [UIHint(UIHint.Textarea)]
        public virtual string SummaryDescription { get; set; }

        [Required]
        [Display(GroupName = GroupNames.Thumbnail, Order = 90, Name = "Thumbnail Image")]
        [UIHint(UIHint.Image)]
        public virtual ContentReference Thumbnail { get; set; }

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
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            Heading = "News";
        }
    }
}