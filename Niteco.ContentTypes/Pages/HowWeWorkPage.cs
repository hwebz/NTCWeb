using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(
       GroupName = GroupNames.Default,
       GUID = "587f3329-47d9-47eb-9861-098d2d347f92")]
    public class HowWeWorkPage : SitePageData
    {
        [Display(
           GroupName = SystemTabNames.Content,
           Order = 320)]
        public virtual ContentArea MainContentArea { get; set; }
    }
}