using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Xml.Linq;
using EPiServer;
using EPiServer.Personalization;
using EPiServer.Personalization.Providers.MaxMind;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using EPiServer.XForms.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Niteco.Common.Extensions;
using Niteco.Common.Mail;
using Niteco.ContentTypes;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.DynamicData;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Helpers;
using Niteco.Web.Models.ViewModels;
using WebMarkupMin.AspNet4.Mvc;
namespace Niteco.Web.Controllers
{
    public class CareerPageController : PageControllerBase<CareerPage>
    {
        private IContentLoader _icontentLoader;

        public CareerPageController(IContentLoader icontentLoader)
        {
            _icontentLoader = icontentLoader;
        }
        [MinifyHtml]
        public ActionResult Index(CareerPage currentPage)
        {
            var md = new CareerPageModel(currentPage);
            
            return View(md);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Submit(string name, string email)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Check user info in database

                    var data = StayUpToDateData.GetByEmail(email);

                    if (data != null) return Json(new { code = "0" }, JsonRequestBehavior.AllowGet);

                    //Save subscriber infor into database
                    var subscriberInfo = new StayUpToDateData{Name = name, Email = email};
                    subscriberInfo.Save();

                    return Json(new { code = "1" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { fail = "failed" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { fail = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
