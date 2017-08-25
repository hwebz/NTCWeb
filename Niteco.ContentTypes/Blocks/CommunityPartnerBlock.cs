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
    [SiteContentType(GUID = "c8f2bc1e-1bd1-48a0-902c-cbff2e050025")]
    public class CommunityPartnerBlock : SiteBlockData
    {
        [Display(Order = 10, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string Title { get; set; }
        
        [AllowedTypes(typeof(CommunityPartnerItemBlock))]
        [Display(GroupName = SystemTabNames.Content, Order = 20)]
        public virtual ContentArea CommunityPartnerItems
        {
            get;
            set;
        }
    }
}
