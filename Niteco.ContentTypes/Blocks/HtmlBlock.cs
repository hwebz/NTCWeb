using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GUID = "e42ccc33-6d4e-4c44-b3d0-0da204fa2a5c")]    
    public class HtmlBlock : SiteBlockData
    {
        [Display(Order = 10, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string Title { get; set; }

        [Display(Order = 20, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string SubTitle { get; set; }

        [Display(Order = 30, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual XhtmlString Description { get; set; }

        [Display(Order = 40, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        [UIHint("textarea")]
        public virtual string RawDescription { get; set; }
    }
}
