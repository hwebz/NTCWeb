using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Web;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.AboutUs, GUID = "84393421-da5e-4c8a-8261-41b8c08273e0")]
    public class AboutUsOfficeBlock : SiteBlockData
    {
        [CultureSpecific]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString Title { get; set; }

        
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual ContentReference ImageBackground { get; set; }


        [Display(Order = 50)]
        [UIHint(UIHint.Image)]
        public virtual Url ImageUrl { get; set; }

        [CultureSpecific]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 3)]
        public virtual XhtmlString Decription { get; set; }
    }
}