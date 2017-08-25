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
    [SiteContentType(GUID = "f016773c-587d-4bf4-86e3-fbf65bdaee79", GroupName = GroupNames.Specialized)]
    [SiteImageUrl]
    public class ServicePage : SitePageData
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

        [Display(GroupName = SystemTabNames.Content, Order = 70)]
        [UIHint(SiteUIHints.TechnologiesList)]
        public virtual string Technologies { get; set; }
    }
}
