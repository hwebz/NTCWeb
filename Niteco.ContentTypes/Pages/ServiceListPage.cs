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
       GUID = "4d78d239-a768-4bcc-b3aa-307afb143abd", GroupName = GroupNames.Default)]
    [SiteImageUrl]
    [AvailableContentTypes(
       Availability.Specific,
       Include = new[] { typeof(ServicePage)})] 
    public class ServiceListPage : SitePageData
    {
        [Display(Order = 60, GroupName = SystemTabNames.Content)]
        [AllowedTypes(typeof(ServicePage))]
        public virtual ContentArea ServiceItems { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 70)]
        public virtual ButtonBlock ContactUs { get; set; }
                  
        public override ContentArea BottomContentArea { get; set; }

        [CultureSpecific]
        [Display(GroupName = SystemTabNames.Content, Order = 80)]
        public virtual XhtmlString XhtmlProp { get; set; }
    }
}
