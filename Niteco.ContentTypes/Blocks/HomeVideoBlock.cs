using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Web;
using Niteco.Common.Consts;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Specialized, GUID ="423a1993-68cd-4222-906d-194fea380f53")]
    public class HomeVideoBlock: SiteBlockData
    {
        [Display(Order = 10)]
        [UIHint(UIHint.Image)]
        public virtual Url LogoImage { get; set; }

        [Display(Order = 20)]
        [CultureSpecific]
        public virtual string Slogan { get; set; }

        [Display(Order = 30)]
        [CultureSpecific]
        public virtual string ButtonText { get; set; }

        [Display(Order = 40)]
        public virtual Url ButtonLink { get; set; }

        [Display(Order = 50)]
        [UIHint(UIHint.Image)]
        public virtual Url BackgroundImage { get; set; }

        [Display(Order = 60)]
        public virtual Url Mp4VideoUrl { get; set; }

        [Display(Order = 70)]
        public virtual Url WebmVideoUrl { get; set; }

        [Display(Order = 80)]
        public virtual Url OggVideoUrl { get; set; } 

    }
}
