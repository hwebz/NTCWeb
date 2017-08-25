using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.XForms;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Blocks;

namespace Niteco.ContentTypes.Pages
{
    [SiteContentType(
       GroupName = GroupNames.Default,
       GUID = "D4444488-9CB0-490F-8FD1-4BD0510ADE66")]
    public class ContactUsPage : SitePageData
    {
        [AllowedTypes(new [] {typeof(OfficeBlock), typeof(OfficeStayConnectedBlock)})]
        [Display(GroupName = SystemTabNames.Content, Order = 70)]
        public virtual ContentArea OfficeItems
        {
            get;
            set;
        }
        [Display(
    Name = "Contact Us Form",
    Description = "This form is used to allow clients contact to",
    GroupName = SystemTabNames.Content,
    Order = 80)]
        [CultureSpecific]
        public virtual XForm ContactUsForm { get; set; }
    }
}
