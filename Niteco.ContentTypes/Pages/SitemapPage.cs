using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(GUID = "3f36e639-43dd-4593-a5a2-50b3dc6d6113", GroupName = GroupNames.Specialized)]
    [SiteImageUrl]
    public class SitemapPage : SitePageData
    {
        [Display(Order = 1)]
        public virtual ContentReference SitemapRoot
        {
            get; set;
        }
    }
}
