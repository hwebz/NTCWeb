using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Pages
{
    /// <summary>
    /// Used to provide on-site search
    /// </summary>
    [SiteContentType(
        GUID = "AAC25733-1D21-4F82-B031-11E626C91E30",
        GroupName = GroupNames.Specialized)]
    [SiteImageUrl]
    public class SearchPage : SitePageData
    {
        [Range(0, 100)]
        public virtual int PageSize { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 310)]
        [CultureSpecific]
        public virtual ContentArea RelatedContentArea { get; set; }
        public virtual ContentReference ContactUsPageLink { get; set; }
        public virtual ContentReference CareerPageLink { get; set; }
        public virtual ContentArea AboutTheCompanyPages { get; set; }

    }
}
