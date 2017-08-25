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
    [SiteContentType(GroupName = GroupNames.AboutUs,
      GUID = "03A61190-9D2B-474B-B29C-DB5819576AAC")]
    public class OurValueItemBlock : SiteBlockData
    {
        [Display(
          GroupName = SystemTabNames.Content,
          Order = 10
        )]
        [CultureSpecific]
        public virtual string ItemTitle { get; set; }

        [Display(
           GroupName = SystemTabNames.Content,
           Order = 20
        )]
        [CultureSpecific]
        public virtual string ItemContent { get; set; }


    }
}
