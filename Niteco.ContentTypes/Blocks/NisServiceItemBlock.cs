using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer;
using EPiServer.Web;

namespace Niteco.ContentTypes.Blocks
{
    [ContentType(DisplayName = "NisServiceItemBlock", GUID = "80b6d94b-af7a-4499-9e03-09071781a3b5", Description = "")]
    public class NisServiceItemBlock : SiteBlockData
    {
        [Display(GroupName = SystemTabNames.Content, Order = 50)]
        [UIHint(UIHint.Image)]
        public virtual Url MainImage { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 60)]
        [CultureSpecific]
        public virtual XhtmlString MainBody { get; set; }
    }
}