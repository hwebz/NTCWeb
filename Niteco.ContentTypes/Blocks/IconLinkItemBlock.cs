using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Web;
using EPiServer;

namespace Niteco.ContentTypes.Blocks
{
    [ContentType(DisplayName = "Icon Link Item Block", GUID = "1dc9dc5a-1da7-4597-a46a-aebc18042221", Description = "Icon Link Item Block")]
    public class IconLinkItemBlock : SiteBlockData
    {
        public virtual string Title { get; set; }

        public virtual Url LinkUrl { get; set; }

        [UIHint(UIHint.Image)]
        public virtual ContentReference Icon { get; set; }

        [UIHint(UIHint.Image)]
        [Display(Name = "Hover Icon")]
        public virtual ContentReference HoverIcon { get; set; }
    }
}