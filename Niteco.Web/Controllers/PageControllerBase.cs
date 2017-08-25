using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc.XForms;
using EPiServer.Web.Routing;
using EPiServer.XForms.Util;
using Newtonsoft.Json;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Business;
using Niteco.Web.Models.ViewModels;
using EPiServer.Web.Mvc;
using EPiServer.Framework.Localization;
using Niteco.Common.Mail;
using Niteco.ContentTypes;

namespace Niteco.Web.Controllers
{
    /// <summary>
    /// All controllers that renders pages should inherit from this class so that we can 
    /// apply action filters, such as for output caching site wide, should we want to.
    /// </summary>
    public abstract class PageControllerBase<T> : PageController<T>, IModifyLayout
        where T : SitePageData
    {

        private readonly XFormPageUnknownActionHandler _xformHandler;
        private string _contentId;

        public PageControllerBase(XFormPageUnknownActionHandler xformHandler)
        {
            _xformHandler = xformHandler;
        }
        public PageControllerBase()
        {
            _xformHandler = new XFormPageUnknownActionHandler();
            _contentId = string.Empty;
        }
        /// <summary>
        /// Signs out the current user and redirects to the Index action of the same controller.
        /// </summary>
        /// <remarks>
        /// There's a log out link in the footer which should redirect the user to the same page. 
        /// As we don't have a specific user/account/login controller but rely on the login URL for 
        /// forms authentication for login functionality we add an action for logging out to all
        /// controllers inheriting from this class.
        /// </remarks>
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        public virtual void ModifyLayout(LayoutModel layoutModel)
        {
            var page = PageContext.Page as SitePageData;
            if (page != null)
            {
                //layoutModel.HideHeader = page.HideSiteHeader;
                //layoutModel.HideFooter = page.HideSiteFooter;
            }
        }

        public string Translate(string resurceKey)
        {
            string value;

            if (!LocalizationService.Current.TryGetString(resurceKey, out value))
            {
                value = resurceKey;
            }

            return value;
        }

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult Success(XFormPostedData xFormPostedData)
        {
            var data = xFormPostedData.XFormFragments.ToList();
            var subject = Translate("/niteco/emailtemplate/subject").Replace("{FullName}", data[0].Value)
                .Replace("\n", "");
            var templateBody = Translate("/niteco/emailtemplate/body");
            var body = templateBody.Replace("{FullName}", data[0].Value)
                .Replace("{Email}", data[1].Value)
                .Replace("{Phone}", data[2].Value)
                .Replace("{Country}", data[3].Value)
                .Replace("{Message}", data[4].Value)
                .Replace("\n", "<br>");

            var emailToSend = SiteSettingsHandler.Instance.SiteSettings.EmailAdmin;
            var emailService = ServiceLocator.Current.GetInstance<IEmailService>();
            if (!string.IsNullOrEmpty(emailToSend))
            {
                emailService.SendEmail(emailToSend, data[0].Value, subject, body, true);
            }

            TempData["xformPosted"] = true;
            return new RedirectResult(Url.Action("Index") + "#contact-form");
            //return RedirectToAction("Index", new { language = PageContext.LanguageID });
        }

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult Failed(XFormPostedData xFormPostedData)
        {
            return new RedirectResult(Url.Action("Index") + "#contact-form");
        }

   

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult XFormPost(XFormPostedData xFormpostedData, string contentId)
        {
            _contentId = contentId;
            //ValidateReCaptcha();
            return _xformHandler.HandleAction(this);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DoSubmit(XFormPostedData xFormpostedData)
        {
            return _xformHandler.HandleAction(this);
        }

      
    }
   
}
