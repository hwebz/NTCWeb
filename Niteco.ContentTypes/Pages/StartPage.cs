using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    /// <summary>
    /// Used for the site's start page and also acts as a container for site settings
    /// </summary>
    [ContentType(
        GUID = "19671657-B684-4D95-A61F-8DD4FE60D559",
        GroupName = GroupNames.Specialized)]
    [SiteImageUrl]
    [AvailableContentTypes(
        Availability.Specific,
        Include = new[] { typeof(ContainerPage), typeof(StandardPage), typeof(SearchPage), typeof(ServiceListPage), typeof(OurTechnologiesPage), typeof(ContactUsPage), typeof(NisPage),
            typeof(SettingPageData), typeof(OurWorkPage), typeof(CareerPage),typeof(AboutUsPage), typeof(HowWeWorkPage),typeof(SitemapPage),typeof(OurTeamPage), typeof(HtmlPage),typeof(NewsListingPage),typeof(BlogListingPage)})] // ...and underneath those we can't create additional start pages
    public class StartPage : SitePageData
    {
        [Display(
          Name = "Setting Page",
          GroupName = GroupNames.SiteSettings,
          Order = 240), Editable(true)]
        [AllowedTypes(typeof(SettingPageData))]
        public virtual PageReference SettingPage { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 320)]
        [AllowedTypes(typeof(SitePageData), typeof(HomeVideoBlock))]
        public virtual ContentArea LeftContentArea { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 330)]
        [AllowedTypes(typeof(SitePageData), typeof(HomeImageBlock))]
        public virtual ContentArea LeftContentAreaForVietNam { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 340)]
        [AllowedTypes(typeof(SitePageData))]
        public virtual ContentArea RightContentArea { get; set; }
    }
}
