using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(
     GUID = "4a4b9b07-5fed-4120-a9ad-4417f49c6341", GroupName = GroupNames.Default)]
    [SiteImageUrl]
    public class OurTechnologiesPage : SitePageData
    {
        [Display(GroupName = SystemTabNames.Content, Order = 70)]
        public virtual ContentArea MainContentArea { get; set; }

        [CultureSpecific]
        [Display(GroupName = SystemTabNames.Content, Order = 80)]
        public virtual XhtmlString XhtmlProp { get; set; }
    }
}
