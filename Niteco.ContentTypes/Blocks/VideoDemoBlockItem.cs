using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [ContentType(DisplayName = "VideoDemoBlockItem", GUID = "707b5063-da72-4ba1-b45d-4599768824d1", Description = "")]
    public class VideoDemoBlockItem : BlockData
    {
        [Display(Order = 10, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string UrlImage { get; set; }

        [Display(Order = 10, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string AlternateText { get; set; }

        [Display(Order = 30, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string UrlVideo { get; set; }
        [Display(Order = 10, GroupName = SystemTabNames.Content)]
        [CultureSpecific]
        public virtual string TitleVideo { get; set; }
    }
}