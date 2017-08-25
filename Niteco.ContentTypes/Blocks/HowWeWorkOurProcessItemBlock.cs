using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(DisplayName = "Our process item block", GUID = "dddfd35d-4926-4801-b606-2f53a5b5b0e7", Description = "", GroupName = GroupNames.HowWeWork)]
    public class HowWeWorkOurProcessItemBlock : SiteBlockData
    {
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual ContentReference ImageIcon { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual ContentReference ImageBackground { get; set; }

        [CultureSpecific]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 5)]

        public virtual string Title{ get; set; }

        [CultureSpecific]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 10)]
        public virtual XhtmlString Description { get; set; }
    }
}