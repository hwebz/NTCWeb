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
using Niteco.ContentTypes.Pages;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Default,
      GUID = "F29D2A25-05A5-4D62-B057-E636D98C8697")]
    public class OurValuesBlock : SiteBlockData
    {
        [Required(ErrorMessage = "Please fill heading of the client quote")]
        [Display(GroupName = SystemTabNames.Content, Order = 10)]
        [CultureSpecific]
        public virtual string Heading { get; set; }
        
        [AllowedTypes(typeof(OurValueItemBlock))]
        [Display(GroupName = SystemTabNames.Content, Order = 20)]
        public virtual ContentArea ValueItems
        {
            get;
            set;
        }
    }
}
