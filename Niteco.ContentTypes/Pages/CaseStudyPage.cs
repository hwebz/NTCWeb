using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Niteco.Common.Consts;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(
        GUID = "1a545abe-2179-4503-afe9-0adb0d47442c")]
    [SiteImageUrl]
    public class CaseStudyPage : StandardPage
    {
        [Display(GroupName = SystemTabNames.Content, Order = 10)]
        [UIHint(UIHint.Image)]
        public virtual Url MainImage { get; set; }

        [Display(GroupName = SystemTabNames.Content, Order = 50)]
        [UIHint(SiteUIHints.ServiceList)]
        public virtual string ServiceTypes { get; set; }

        public List<SitePageData> ListServicePage
        {
            get
            {
                var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
                var listPage = new List<SitePageData>();
                if (!string.IsNullOrEmpty(this.ServiceTypes))
                {
                    var listServiceType = this.ServiceTypes.Split(',');
                    foreach (var item in listServiceType)
                    {
                        var contentReference = new ContentReference(item);
                        SitePageData pageData;
                        if (!ContentReference.IsNullOrEmpty(contentReference) && contentLoader.TryGet<SitePageData>(contentReference,out pageData))
                        {
                            listPage.Add(pageData);
                        }
                    }
                }
                return listPage;
            }
        }
        public virtual ContentArea CaseStudyDetailItems { get; set; }
    }
}
