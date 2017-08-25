using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(
       GUID = "64d0f4b8-c5f5-40d6-a9f1-6b974ef66acd")]
    [SiteImageUrl]
    public class OurTeamPage: SitePageData
    {
        [Display(Order = 60)]
        [AllowedTypes(typeof(PeopleBlock))]
        public virtual ContentArea TeamsContentArea { get; set; }
    }
}
