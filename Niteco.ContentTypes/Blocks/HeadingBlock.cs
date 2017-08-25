using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using Niteco.Common.Consts;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Default, GUID = "a44c06b6-7925-4fb6-9637-00975ab76392")]
    public class HeadingBlock : BlockData
    {

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 10
        )]
        [Required(ErrorMessage = "Please fill heading of the heading page")]
        [CultureSpecific]
        public virtual string Heading { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 20
        )]
        [CultureSpecific]
        public virtual string Description { get; set; }

        [Display(Order = 30)]
        [UIHint(UIHint.Image)]
        public virtual Url BackgroundUrl { get; set; }
    }
}