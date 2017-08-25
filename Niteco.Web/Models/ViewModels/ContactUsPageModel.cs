using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Models.ViewModels
{
    public class ContactUsPageModel : PageViewModel<ContactUsPage>
    {
        public ContactUsPageModel(ContactUsPage currentPage) : base(currentPage)
        {
            
        }
        public string Locations { get; set; }
        public string ActionUrl { get; set; }
    }
}