using System;
using System.ComponentModel.DataAnnotations;
using System.Web.UI.WebControls;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Blocks
{
    [ContentType(DisplayName = "Partner ship block", GUID = "2d2a4a11-36ae-4723-abc1-902f71eb11f8", Description = "", GroupName = GroupNames.HowWeWork)]
    public class HowWeWorkPartnershipBlock : SiteBlockData
    {
        [CultureSpecific]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual string Title { get; set; }

        
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 2)]
        [AllowedTypes(typeof(HtmlBlock))]
        public virtual ContentArea PartnerShipItem { get; set; }

        [Display(Order = 3)]
        [CultureSpecific]
        public virtual XhtmlString MainContent { get; set; }

        [Display(Order = 4)]
        public virtual ButtonBlock ContactUs { get; set; }
    }
}