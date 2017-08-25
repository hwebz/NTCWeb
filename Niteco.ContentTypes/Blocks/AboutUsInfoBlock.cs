using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.AboutUs,
      GUID = "0CDC4CA7-9092-4616-80E2-9594947B095A")]
    public class AboutUsInfoBlock : SiteBlockData
    {
        [AllowedTypes(typeof(AboutUsInfoItemBlock))]
        [Display(GroupName = SystemTabNames.Content, Order = 10)]
        public virtual ContentArea AboutUsInfoItems
        {
            get;
            set;
        }
    }
}
