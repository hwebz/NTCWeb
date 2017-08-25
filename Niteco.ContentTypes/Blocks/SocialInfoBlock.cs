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
using Niteco.ContentTypes.Media;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.AboutUs, GUID = "b7c1d6af-5090-490d-a45c-140025315224")]
    [SiteImageUrl]
    public class SocialInfoBlock : SiteBlockData
    {
        [Display(Order = 10, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string Title { get; set; }

        [Display(Order = 20, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string SubTitle { get; set; }

        [Display(Order = 30, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual XhtmlString ShortDescription { get; set; }

        [Display(Order = 40, GroupName = SystemTabNames.Content)]
        [AllowedTypes(typeof(ImageFile))]
        public virtual ContentArea ImageContentArea { get; set; }

        [Display(Order = 50, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual XhtmlString MainBody { get; set; }
    }
}
