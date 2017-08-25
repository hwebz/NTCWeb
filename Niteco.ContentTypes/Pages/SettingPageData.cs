using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.Media;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(GUID = "d8b50956-2e28-4503-97c6-6f9eca683e7b", GroupName = GroupNames.SiteSettings, Order = 100)]
    [SiteImageUrl]
    public class SettingPageData : PageData, IContainerPage, ISiteSettings
    {
        [Display(GroupName = GroupNames.Header, Order = 5)]
        [UIHint(UIHint.Image)]
        public virtual Url LogoImage { get; set; }

        [CultureSpecific]
        [Display(GroupName = GroupNames.Header, Order = 5)]
        public virtual string Slogan { get; set; }

        [Display(GroupName = GroupNames.Header, Order = 10)]
        [AllowedTypes(typeof(SitePageData))]
        public virtual ContentArea MenuItemsContent { get; set; }

        [Display(GroupName = GroupNames.Header, Order = 20)]
        public virtual Url FacebookLink { get; set; }

        [Display(GroupName = GroupNames.Header, Order = 20)]
        public virtual Url TwitterLink { get; set; }

        [Display(GroupName = GroupNames.Header, Order = 30)]
        public virtual Url LinkedinLink { get; set; }

        [Display(GroupName = GroupNames.Header, Order = 31)]
        public virtual string Email { get; set; }

        [Display(GroupName = GroupNames.SiteSettings, Order = 10)]
        [AllowedTypes(typeof(SearchPage))]
        public virtual ContentReference SearchPageLink { get; set; }

        [Display(GroupName = GroupNames.SiteSettings, Order = 20)]
        [AllowedTypes(typeof(ServiceListPage))]
        public virtual ContentReference ServiceListRoot { get; set; }

        [Display(GroupName = GroupNames.SiteSettings, Order = 40)]
        [AllowedTypes(typeof(OfficeBlock))]
        public virtual ContentArea FooterItemsContent { get; set; }

        [AllowedTypes(typeof(ImageFile))]
        [Display(
            GroupName = GroupNames.Footer)]
        public virtual ContentArea PartnerImages { get; set; }

        [Display(
            GroupName = GroupNames.Footer)]

        public virtual string FooterCopyright { get; set; }

        [Display(
            Name = "Site-wide head scripts",
            GroupName = GroupNames.SiteSettings,
            Order = 550)]
        [UIHint(UIHint.Textarea)]
        public virtual string GlobalHeadScripts { get; set; }

        [Display(
            Name = "Site-wide body scripts",
            GroupName = GroupNames.SiteSettings,
            Order = 560)]
        [UIHint(UIHint.Textarea)]
        public virtual string GlobalBodyScripts { get; set; }

        [Display(
            Name = "Site-wide footer scripts",
            GroupName = GroupNames.SiteSettings,
            Order = 570)]
        [UIHint(UIHint.Textarea)]
        public virtual string GlobalFooterScripts { get; set; }

        [Display(
            Name = "Contact email ",
            GroupName = GroupNames.SiteSettings,
            Order = 580)]
        public virtual string EmailAdmin { get; set; }

        [Display(
            Name = "Assets minified version",
            GroupName = GroupNames.SiteSettings,
            Order = 590)]
        public virtual string AssetsVersion { get; set; }

        [Display(
            Name = "List Country ",
            GroupName = GroupNames.SiteSettings,
            Order = 600)]
        [UIHint(UIHint.Textarea)]
        public virtual string ListCountry { get; set; }

        [Display(Name = "Sendgrid Api Key", GroupName = GroupNames.SiteSettings, Order = 610)]
        public virtual string SendgridApiKey { get; set; }

        [Display(Name = "Mail Chimp Api Key", GroupName = GroupNames.Subscription, Order = 10)]
        public virtual string MailChimpApiKey { get; set; }

        [Display(Name = "Mail Chimp List Id", GroupName = GroupNames.Subscription, Order = 20)]
        public virtual string MailChimpListId { get; set; }

        [Display(Name = "Subscription Title", GroupName = GroupNames.Subscription, Order = 30)]
        public virtual string SubscriptionText { get; set; }

        [Display(Name = "Subscribe Sub title", GroupName = GroupNames.Subscription, Order = 40)]
        public virtual string SubscriptionDescription { get; set; }
        [Display(Name = "Subcription Url Post", GroupName = GroupNames.Subscription, Order = 40)]
        public virtual string SubcriptionUrlPost { get; set; }

        [Display(Name = "Comment Config", GroupName = GroupNames.CommentConfig, Order = 40)]
        public virtual string CommentConfig { get; set; }

        [Display(Name = "Author Container", GroupName = GroupNames.Subscription, Order = 50)]
        public virtual ContentReference AuthorContainer { get; set; }
    }
}
