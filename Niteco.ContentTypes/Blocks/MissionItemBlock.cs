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
      GUID = "A77282BD-BAB1-4044-A6DF-ECA2B1A1512A")]
    public class MissionItemBlock : SiteBlockData
    {
        [Display(
          GroupName = SystemTabNames.Content,
          Order = 10
        )]
        [CultureSpecific]
        public virtual string MissionHeading { get; set; }

        [Display(
           GroupName = SystemTabNames.Content,
           Order = 20
        )]
        [CultureSpecific]
        public virtual string MissionContent { get; set; }

        [Display(Order = 30)]
        [UIHint(UIHint.Image)]
        public virtual Url Image { get; set; }

        [Display(Order = 40)]
        public virtual bool ImageFlyUp { get; set; }

    }
}
