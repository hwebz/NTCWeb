using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Web;
using EPiServer.DataAbstraction;

namespace Niteco.ContentTypes.Blocks
{
    [ContentType(DisplayName = "NewsQuoteBlock", GUID = "5914cd07-8eaf-4b6d-bca7-62e7a709fe33", Description = "Quote block")]
    public class NewsQuoteBlock : SiteBlockData
    {
        [Display(GroupName = SystemTabNames.Content, Order = 10)]
        [UIHint(UIHint.Image)]
        public virtual ContentReference Image { get; set; }

        [Required]
        [UIHint(UIHint.LongString)]
        [Display(GroupName = SystemTabNames.Content, Order = 20)]
        public virtual string Quote { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 30, Name = "Sub Quote")]
        public virtual XhtmlString SubQuote { get; set; }
    }
}