using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GUID = "c56d419f-e275-46cc-894e-5a7f21c077e7")]
    public class NisButtonBlock : SiteBlockData
    {
        [Display(Order = 10)]
        [CultureSpecific]
        public virtual XhtmlString Description { get; set; }

        [Display(Order = 20)]
        [CultureSpecific]
        public virtual string ButtonText { get; set; }

        [Display(Order = 30)]
        public virtual Url ButtonLink { get; set; }

        [Display(Order = 40)]
        [CultureSpecific]
        public virtual string Button2Text { get; set; }

        [Display(Order = 50)]
        public virtual Url Button2Link { get; set; }
    }
}