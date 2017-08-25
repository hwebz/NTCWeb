using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.Common.Enums;
using Niteco.ContentTypes.EditorDescriptors;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GUID = "c8f2bc1f-1bd1-48a0-902c-cbff2e050025")]
    public class TechnologyPartnerBlock : SiteBlockData
    {
        [Display(Order = 10, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string Title { get; set; }

        [Display(Order = 20)]
        [CultureSpecific]
        public virtual XhtmlString ShortDescription { get; set; }

        [AllowedTypes(typeof(TechnologyPartnerItemBlock))]
        [Display(GroupName = SystemTabNames.Content, Order = 30)]
        public virtual ContentArea TechnologyPartnerItems{ get; set; }

        [Display(Order = 40)]
        [CultureSpecific]
        public virtual string BecomeOurPartnerText { get; set; }

        [Display(Order = 50)]
        [CultureSpecific]
        public virtual ContentReference BecomeOurPartnerLink { get; set; }
    }
}
