using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.Common.Enums;
using Niteco.ContentTypes.EditorDescriptors;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Default,
      GUID = "4209B31F-949C-4EB8-BB94-D0E4C41F3101")]
    public class ClientQuoteItemBlock : SiteBlockData
    {

        [Display(
          GroupName = SystemTabNames.Content,
          Order = 20
        )]
        [CultureSpecific]
        public virtual XhtmlString Description { get; set; }
      
        [Display(
        GroupName = SystemTabNames.Content,
        Order = 30
         )]
        [CultureSpecific]
        public virtual string ClientPersonName { get; set; }

        [Display(
        GroupName = SystemTabNames.Content,
        Order = 40
        )]
        [CultureSpecific]
        public virtual string ClientPersonTitle { get; set; }

        [Display(Order = 50)]
        [UIHint(UIHint.Image)]
        public virtual Url ImageUrl { get; set; }

    }
}
