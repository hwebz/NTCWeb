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
    [SiteContentType(GUID = "ae867ee9-23a4-42c9-8ef7-036dc4851ff2")]
    [SiteImageUrl]
    public class OfficeBlock : SiteBlockData
    {
        [Display(Order = 10)]
        public virtual string OfficeName { get; set; }

        [CultureSpecific]
        [Display(Order = 20)]
        [UIHint(UIHint.LongString)]
        public virtual string Address { get; set; }

        [Display(Order = 30)]
        public virtual Double Latitude { get; set; }

        [Display(Order = 40)]
        public virtual Double Longitude { get; set; }

        [Display(Order = 50)]
        public virtual string Email { get; set; }

        [Display(Order = 60)]
        public virtual string Phone { get; set; }

        [Display(Order = 70)]
        [UIHint(UIHint.Image)]
        public virtual Url OfficeImage { get; set; }

        [Display(Order = 80)]
        public virtual bool IsInfoLeft { get; set; }
    }
}
