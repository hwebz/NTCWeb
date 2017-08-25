using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer;
using EPiServer.Web;


namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GUID = "ba320409-02cb-40d5-896e-88d0b181bdcb")]
    public class NisVerifyByEpiServer : SiteBlockData
    {
        [CultureSpecific]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 1)]

        public virtual XhtmlString Title { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual ContentReference ImageBackground { get; set; }


        [Display(Order = 50)]
        [UIHint(UIHint.Image)]
        public virtual Url ImageUrl { get; set; }

        [CultureSpecific]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 3)]
        public virtual XhtmlString Decription { get; set; }
    }
}