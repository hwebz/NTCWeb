using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(
       GUID = "da9a5247-5bd5-409e-b71b-be231ce40d8b")]
    [SiteImageUrl]
    public class OurWorkPage: SitePageData
    {
        [Display(GroupName = SystemTabNames.Content, Order = 60)]
        [AllowedTypes(typeof(SitePageData))]
        public virtual ContentArea ProjectsContentArea { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 70)]
        [AllowedTypes(typeof(ClientQuoteItemBlock))]
        public virtual ContentArea ClientQuotesContentArea { get; set; }
    }
}
