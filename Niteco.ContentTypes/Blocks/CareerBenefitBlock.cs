using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Web;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Blocks
{
    [SiteContentType(GroupName = GroupNames.Career, GUID = "8c811832-431e-4b5e-ad00-19f1216cf84d")]
    public class CareerBenefitBlock: HtmlBlock
    {
        [Display(Order = 40)]
        [UIHint(UIHint.Image)]
        public virtual Url MainImage { get; set; }
    }
}
