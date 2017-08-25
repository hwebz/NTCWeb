using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Web;

namespace Niteco.ContentTypes.Blocks
{
	[ContentType(DisplayName = "CodeWrapBlock", GUID = "7ad43cc1-874c-4086-9249-ff33f612ba40", Description = "")]
	public class CodeWrapBlock : SiteBlockData
	{
		[CultureSpecific]
		[Display(
			Name = "Title",
			GroupName = SystemTabNames.Content,
			Order = 1)]
		public virtual string Title { get; set; }

		[CultureSpecific]
		[Required]
		[UIHint(UIHint.LongString)]
		[Display(
					Name = "Content",
					GroupName = SystemTabNames.Content,
					Order = 2)]
		public virtual string Content { get; set; }

	    public override string SearchText
	    {
            get { return Content; }
	    }
	}
}
