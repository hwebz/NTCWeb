using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;
using EPiServer.Web;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Career, GUID = "24025393-f9b7-4693-9161-1a3e46b2cb3b")]
    [SiteImageUrl]
    public class ApplyCareerBlock : ButtonBlock
    {
        [Display(Order = 1)]
        public virtual HtmlBlock ApplyContent { get; set; }

        [Display(Order = 5)]
        [UIHint(UIHint.Image)]
        public virtual Url ApplyCareerBackgroundImage { get; set; }

        [Display(Order = 10)]
        [AllowedTypes(typeof(HtmlBlock))]
        public virtual ContentArea Steps { get; set; }
    }
}
