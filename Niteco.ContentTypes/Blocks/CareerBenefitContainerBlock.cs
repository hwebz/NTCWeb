using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Niteco.ContentTypes.Blocks
{
    [ContentType(AvailableInEditMode = false, GUID = "554FEA89-A4C7-47DA-A298-F4041C7E6DCA")]
    [SiteImageUrl]
    public class CareerBenefitContainerBlock : SiteBlockData
    {
        [Display(Order = 10)]
        [CultureSpecific]
        public virtual string Title { get; set; }

        [Display(Order = 20)]
        [CultureSpecific]
        public virtual XhtmlString ShortDescription { get; set; }

        [Display(Order = 30)]
        [AllowedTypes(typeof(CareerBenefitBlock))]
        public virtual ContentArea CareerBenefits { get; set; }
    }
}
