using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [ContentType(DisplayName = "VideoDemoBlock", GUID = "51d9ea34-4cf4-4382-9a95-a16453df284c", Description = "")]
    public class VideoDemoBlock : BlockData
    {
        [AllowedTypes(typeof(VideoDemoBlockItem))]
        [Display(GroupName = SystemTabNames.Content, Order = 20)]
        public virtual ContentArea VideoItems
        {
            get;
            set;
        }
    }
}