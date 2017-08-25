using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(DisplayName = "Our process block", GUID = "36e0cad3-dc22-4293-9e9b-7c529252da09", Description = "", GroupName = GroupNames.HowWeWork)]
    public class HowWeWorkOurProcessBlock : SiteBlockData
    {
        [CultureSpecific]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual string Title { get; set; }

        [CultureSpecific]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual String Description { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 3)]
        [AllowedTypes(typeof(HowWeWorkOurProcessItemBlock))]
        public virtual ContentArea OurProcessItem { get; set; }
    }
}