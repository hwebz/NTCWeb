using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Specialized, GUID = "{1DCE5223-B14A-4484-8EB2-5EA3FF803AE1}")]
    public class HomeImageBlock: SiteBlockData
    {
        [Display(Order = 100)]
        public virtual Url BackgroundImage { get; set; }

        [Display(Order = 110)]
        public virtual Url BillboardImage { get; set; }

        [Display(Order = 120)]
        [CultureSpecific]
        public virtual string LinkText { get; set; }

        [Display(Order = 130)]
        public virtual Url Link { get; set; }
    }
}
