using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using Niteco.Common.Consts;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.AboutUs,
      GUID = "1E92AC97-52C0-447A-AF45-082360A7DA12")]
    public class AboutUsInfoItemBlock : SiteBlockData
    {
        [Display(
          GroupName = SystemTabNames.Content,
          Order = 10
        )]
        [CultureSpecific]
        public virtual string InfoTitle { get; set; }

        [Display(
           GroupName = SystemTabNames.Content,
           Order = 20
        )]
        [CultureSpecific]
        public virtual string InfoNumber { get; set; }

        [Display(Order = 30)]
        [UIHint(UIHint.Image)]
        public virtual Url InfoIcon { get; set; }


    }
}
