using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [ContentType(DisplayName = "AuthorPage", GUID = "90570b69-9625-4d16-988b-6b81018ed632", Description = "Author page")]
    public class AuthorPage : PageData
    {
        [Required]
        [Display(Name = "Author Name")]
        public virtual string AuthorName { get; set; }

        public virtual string Position { get; set; }

        [UIHint(UIHint.Textarea)]
        public virtual string Description { get; set; }

//        [Display(Name = "Social Account Links")]
//        [AllowedTypes(typeof(IconLinkItemBlock))]
//        public virtual ContentArea SocialAccountLinks { get; set; }

        [Display(Name = "Image", GroupName = SystemTabNames.Content, Order = 20)]
        public virtual ContentReference Thumbnail { get; set; }

        [Display(Name = "Email Link", GroupName = GroupNames.Social, Order = 20)]
        public virtual Url EmailToLink { get; set; }
        [Display(Name = "Twitter Link", GroupName = GroupNames.Social, Order = 20)]
        public virtual Url TwitterLink { get; set; }
        [Display(Name = "Linkedin Link", GroupName = GroupNames.Social, Order = 20)]
        public virtual Url LinkedinLink { get; set; }
        [Display(Name = "Facebook Link", GroupName = GroupNames.Social, Order = 20)]
        public virtual Url FacebookLink { get; set; }
        [Display(Name = "Stackover flow Link", GroupName = GroupNames.Social, Order = 20)]
        public virtual Url StackoverflowLink { get; set; }
        [Display(Name = "Episerver Link", GroupName = GroupNames.Social, Order = 20)]
        public virtual Url EpiserverLink { get; set; }


    }
}