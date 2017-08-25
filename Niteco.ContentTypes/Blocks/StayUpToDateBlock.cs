using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Specialized, GUID = "0BFABE14-D845-4BB8-B178-71352354BE05")]
    public class StayUpToDateBlock : SiteBlockData
    {
        [Display(Order = 10)]
        public virtual Url StayUpToDateImage { get; set; }

        [Display(Order = 20)]
        [CultureSpecific]
        public virtual string StayUpToDateHeader { get; set; }

        [Display(Order = 30)]
        [CultureSpecific]
        public virtual XhtmlString StayUpToDateContent { get; set; }
        
        [Display(Order = 40)]
        [CultureSpecific]
        public virtual XhtmlString StayUpToDateVourcher { get; set; }

        [Display(Order = 50)]
        [CultureSpecific]
        public virtual string StayUpToDateButtonText { get; set; }
    }
}
