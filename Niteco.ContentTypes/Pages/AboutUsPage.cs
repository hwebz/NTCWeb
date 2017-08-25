using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Web;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(
        GUID = "fff16a1f-8400-4b29-b558-dfd3d6910cb8")]
    [SiteImageUrl]
    public class AboutUsPage : SitePageData
    {
       
        [Display(GroupName = SystemTabNames.Content, Order = 70)]
        public virtual ContentArea MainContentArea
        {
            get;
            set;
        }

    }
}
