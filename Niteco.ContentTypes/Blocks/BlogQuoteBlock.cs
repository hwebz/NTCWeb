using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Web;

namespace Niteco.ContentTypes.Blocks
{
	[ContentType(DisplayName = "BlogQuoteBlock", GUID = "a1b164a5-afb7-4e98-9d71-827ae5419630", Description = "")]
	public class BlogQuoteBlock : SiteBlockData
	{
		[CultureSpecific]
		[Display(
			Name = "Content Quote",
			GroupName = SystemTabNames.Content,
			Order = 1)]
		[Required]
		[UIHint(UIHint.LongString)]
		public virtual string Quote { get; set; }

	    public override string SearchText
	    {
            get { return Quote; }
	    }
	}
}
