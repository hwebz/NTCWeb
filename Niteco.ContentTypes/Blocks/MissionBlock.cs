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
    [SiteContentType(GroupName = GroupNames.AboutUs,
      GUID = "9EA7D7D0-5D17-4B4B-9F22-C2AA70078678")]
    public class MissionBlock : SiteBlockData
    {
        [AllowedTypes(typeof(MissionItemBlock))]
        [Display(GroupName = SystemTabNames.Content, Order = 10)]
        public virtual ContentArea MissionItems
        {
            get;
            set;
        }
    }
}
