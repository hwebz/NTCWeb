using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using Niteco.Common.Consts;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(GUID = "9924b8fc-afeb-4f04-a99f-7b600bcf7e57", GroupName = GroupNames.Specialized)]
    [SiteImageUrl]
    public class TechnologyPage : SitePageData
    {
        [Display(GroupName = SystemTabNames.Content, Order = 50)]
        [UIHint(UIHint.Image)]
        public virtual Url MainImage { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 51)]
        [UIHint(UIHint.Image)]
        public virtual Url LargeImage { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 60)]
        [CultureSpecific]
        public virtual XhtmlString MainBody { get; set; }
    }
}
