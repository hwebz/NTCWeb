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
      GUID = "45A3FCF3-BBF7-46E4-B458-F7C83BBBEC7A")]
    public class CaseStudyDetailBlock : SiteBlockData
    {
        [Display(
          GroupName = SystemTabNames.Content,
          Order = 10
        )]
        [CultureSpecific]
        public virtual string Heading { get; set; }

        [Display(
        GroupName = SystemTabNames.Content,
        Order = 20
      )]
        [CultureSpecific]
        public virtual XhtmlString ContentDescription { get; set; }

      

        [Display(Order = 30)]
        [UIHint(UIHint.Image)]
        public virtual Url Image { get; set; }

        [Display(Order = 40)]
        [UIHint(UIHint.Image)]
        public virtual Url LongImage { get; set; }
    }
}
