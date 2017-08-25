using System.Web.Mvc;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Models.ViewModels;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using WebMarkupMin.AspNet4.Mvc;

namespace Niteco.Web.Controllers
{
    public class StartPageController : PageControllerBase<StartPage>
    {
        [MinifyHtml]
        public ActionResult Index(StartPage currentPage)
        {
            var model = PageViewModel.Create(currentPage);

            if (SiteDefinition.Current.StartPage.CompareToIgnoreWorkID(currentPage.ContentLink)) // Check if it is the StartPage or just a page of the StartPage type.
            {
                //Connect the view models logotype property to the start page's to make it editable
                var editHints = ViewData.GetEditHints<PageViewModel<StartPage>, StartPage>();
                //editHints.AddConnection(m => m.Layout.Logotype, p => p.SiteLogotype);
               
            }

            return View(model);
        }

    }
}
