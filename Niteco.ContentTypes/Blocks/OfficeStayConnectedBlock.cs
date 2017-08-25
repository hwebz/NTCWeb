using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Web;
using Niteco.Common.Attributes;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GUID = "93174EDD-EDEF-4235-A7B2-4441CAF48366")]
    [SiteImageUrl]
    public class OfficeStayConnectedBlock : OfficeBlock
    {
        [Display(Order = 20)]
        [UIHint(UIHint.LongString)]
        [CultureSpecific]
        public virtual string ShortDescription { get; set; }
    }
}
