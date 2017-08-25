using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using Niteco.Common.Consts;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(
        GUID = "4191ae7b-a011-4176-bf7f-6182933d232a", GroupName = GroupNames.Career)]
    public class JobPage: SitePageData
    {
        [Display(Order = 60, GroupName = GroupNames.Teaser)]
        [UIHint(UIHint.Image)]
        public virtual Url TeaserImage { get; set; }

        [Display(Order = 70)]
        [CultureSpecific]
        public virtual string WorkLocation { get; set; }

        [Display(Order = 80)]
        [CultureSpecific]
        public virtual XhtmlString JobContent { get; set; }

        [ScaffoldColumn(false)]
        public override ContentArea BottomContentArea { get; set; }
    }
}
