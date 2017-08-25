using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GUID ="ce618090-c74f-4027-95fd-0c328505c846")]
    [SiteImageUrl]
    public class ButtonBlock:SiteBlockData
    {
        [Display(Order = 10)]
        [CultureSpecific]
        public virtual XhtmlString Description { get; set; }

        [Display(Order=20)]
        [CultureSpecific]
        public virtual string ButtonText { get; set; }

        [Display(Order = 30)]
        public virtual Url ButtonLink { get; set; }
    }
}
