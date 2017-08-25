using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.Common.Enums;
using Niteco.ContentTypes.EditorDescriptors;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.AboutUs,
      GUID = "BF8AA3E2-77A0-48B9-A81A-C0DAFD96D5E2")]
    public class TechnologyPartnerItemBlock : SiteBlockData
    {
        [Display(Order = 10, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string Title { get; set; }

        [Display(Order = 20)]
        [UIHint(UIHint.Image)]
        public virtual Url ImageUrl { get; set; }

        [Display(Order = 30, GroupName = SystemTabNames.Content)]
        public virtual Url LinkContent { get; set; }
    }
}
