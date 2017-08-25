using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace Niteco.ContentTypes.Pages
{
    
    [SiteContentType(GUID = "2d3e1eb4-0268-4cf9-aacb-daa31eca972f")]
    public class NisPage : SitePageData
    {
        [Display(GroupName = SystemTabNames.Content, Order = 70)]
        public virtual ContentArea MainContentArea
        {
            get;
            set;
        }

    }
}