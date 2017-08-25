using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using Niteco.ContentTypes.Blocks;
using EPiServer;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(
        GUID = "7275e56b-24c5-4994-b6d5-c7c359b5c787")]
    [SiteImageUrl]
    public class CareerPage: SitePageData
    {
        [ScaffoldColumn(false)]
        public override HtmlBlock MainContent { get; set; }

        [Display(Order = 70)]
        public virtual ApplyCareerBlock ApplySteps { get; set; }

        [Display(Order = 80)]
        public virtual CareerBenefitContainerBlock CareerBenefit { get; set; }

        [Display(Order = 87)]
        public virtual Url JobOpeningBackground { get; set; }

        [Display(Order = 90)]
        [AllowedTypes(typeof(JobPage))]
        public virtual ContentArea JobOpenings { get; set; }

        [Display(Order = 100)]
        public virtual ButtonBlock ContactUs { get; set; }

        [Display(Order = 110)]
        public virtual StayUpToDateBlock StayUpToDate { get; set; }
    }
}
