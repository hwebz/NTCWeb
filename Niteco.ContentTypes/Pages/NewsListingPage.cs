using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Core;
using EPiServer.Web;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [ContentType(DisplayName = "NewsListingPage", GUID = "60180ce1-60c2-485f-8f6e-85b354313c3a")]
    [AvailableContentTypes(Availability.Specific, Include = new[] { typeof(NewsDetailPage) })] // ...and underneath those we can't create additional start pages
    public class NewsListingPage : SitePageData
    {
        [Display(Name = "Number of items per page", GroupName = SystemTabNames.Content)]
        [Range(1, int.MaxValue)]
        public virtual int ItemsPerLoad { get; set; }

        [Display(Name = "Loading Image", GroupName = SystemTabNames.Content)]
        [UIHint(UIHint.Image)]
        public virtual ContentReference LoadingImage { get; set; }

        [Display(Name = "Loading Text", GroupName = SystemTabNames.Content)]
        public virtual string LoadingText { get; set; }

        [ScaffoldColumn(false)]
        public override HtmlBlock MainContent { get; set; }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            ItemsPerLoad = 5;
        }
    }
}