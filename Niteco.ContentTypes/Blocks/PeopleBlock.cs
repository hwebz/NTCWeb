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
using Niteco.Common.Consts;
using Niteco.ContentTypes.Pages;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Default,
      GUID = "3F16F004-6216-4BE7-A5E7-C7CC9777BFD4")]
    public class PeopleBlock : SiteBlockData
    {
        [Display(
           GroupName = SystemTabNames.Content,
           Order = 10
         )]
        [Required(ErrorMessage = "Please fill the name of person")]

        public virtual string PersonName { get; set; }

        [Display(
           GroupName = SystemTabNames.Content,
           Order = 20
         )]
        [CultureSpecific]
        public virtual string StaffTitle { get; set; }

        [Display(Order = 25)]
        [UIHint(UIHint.Image)]
        public virtual Url PersonImage { get; set; }

        [Display(Order = 30)]
        [CultureSpecific]
        public virtual XhtmlString Description { get; set; }
    }
}
