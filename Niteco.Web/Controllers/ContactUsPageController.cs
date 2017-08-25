using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Newtonsoft.Json;
using Niteco.Common.Mail;
using Niteco.ContentTypes;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.DynamicData;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Helpers;
using Niteco.Web.Models.ViewModels;
using EPiServer.Logging;
using System.Threading.Tasks;
using WebMarkupMin.AspNet4.Mvc;

namespace Niteco.Web.Controllers
{
    public class ContactUsPageController : PageControllerBase<ContactUsPage>
    {
        private IContentLoader _icontentLoader;
        private static readonly ILogger Logger = LogManager.GetLogger();

        public ContactUsPageController(IContentLoader icontentLoader)
        {
            _icontentLoader = icontentLoader;
        }

        [MinifyHtml]
        public ActionResult Index(ContactUsPage currentPage)
        {
            var md = new ContactUsPageModel(currentPage);
            var locations = string.Empty;
            if (currentPage.OfficeItems != null && currentPage.OfficeItems.Items.Any())
            {
                for (var i = 0; i < currentPage.OfficeItems.Items.Count; i++)
                {
                    var item = currentPage.OfficeItems.Items[i];
                    var officeBlock = _icontentLoader.Get<OfficeBlock>(item.ContentLink);
                    if (!Double.IsNaN(officeBlock.Latitude) && !officeBlock.Latitude.Equals(0) &&
                        !Double.IsNaN(officeBlock.Longitude) && !officeBlock.Longitude.Equals(0))
                    {
                        var str = string.Format("['{0}',{1},{2},{3}]", HttpUtility.HtmlEncode(officeBlock.Address),
                            officeBlock.Latitude,
                            officeBlock.Longitude, i + 1);
                        locations = !string.IsNullOrEmpty(locations) ? string.Format("{0},{1}", locations, str) : str;
                    }

                }
            }
            md.Locations = string.Format("[{0}]", locations);
            if (currentPage != null && currentPage.ContactUsForm != null)
            {
                var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
                var pageUrl = urlResolver.GetUrl(currentPage.ContentLink);
                var actionUrl = string.Format("{0}XFormPost/", pageUrl);
                actionUrl = UriSupport.AddQueryString(actionUrl, "XFormId", currentPage.ContactUsForm.Id.ToString());
                actionUrl = UriSupport.AddQueryString(actionUrl, "failedAction", "Failed");
                actionUrl = UriSupport.AddQueryString(actionUrl, "successAction", "Success");
                actionUrl = UriSupport.AddQueryString(actionUrl, "contentId",
                    currentPage.ContentLink.ID.ToString(CultureInfo.InvariantCulture));
                md.ActionUrl = actionUrl;
            }
            ViewBag.Country = LocationHelpers.GetCountryCodeByIp();
            var result = GetPhoneCodeByCountryCode(ViewBag.Country);
            ViewBag.PhoneCode = result.Data.phone;
            ViewBag.ListCountry = GetAllCountries();
            return View(md);
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> Submit(ContactFormModel model)
        {
            try
            {
                if (!ValidateReCaptcha())
                {
                    return Json(new {status = "invalidRecapcha"}, JsonRequestBehavior.AllowGet);
                }
                if (ModelState.IsValid)
                {
                    var contactFormData = new ContactFormData();
                    contactFormData.FullName = model.FullName;
                    contactFormData.Phone = model.Phone;
                    contactFormData.Email = model.Email;
                    contactFormData.Country = model.Country;
                    contactFormData.Message = model.Message;
                    contactFormData.Save();
                    //send email
                    var subject = Translate("/niteco/emailtemplate/subject").Replace("{FullName}", model.FullName)
                        .Replace("\n", "");
                    var templateBody = Translate("/niteco/emailtemplate/body");
                    var body = templateBody.Replace("{FullName}", model.FullName)
                        .Replace("{Email}", model.Email)
                        .Replace("{Phone}", model.Phone)
                        .Replace("{Country}", model.Country)
                        .Replace("{Message}", model.Message)
                        .Replace("\n", "<br>");

                    var emailToSend = SiteSettingsHandler.Instance.SiteSettings.EmailAdmin;
                    var emailService = ServiceLocator.Current.GetInstance<IEmailService>();
                    if (!string.IsNullOrEmpty(emailToSend))
                    {
                        //emailService.SendEmail(emailToSend, model.FullName, subject, body, true);
                        await emailService.SendSendgridEmailAsync(emailToSend, model.Email, model.FullName, subject, body,
                            SiteSettingsHandler.Instance.SiteSettings.SendgridApiKey);
                    }

                    return Json(new {status = "sucess"}, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = "failed" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new {status = "failed"}, JsonRequestBehavior.AllowGet);
            }
        }
        public bool ValidateReCaptcha()
        {
            var result = Request.Form["g-recaptcha-response"];

            var url = string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}",

                ConfigurationManager.AppSettings["ReCaptchaSiteSecret"],
                result);

            var request = HttpWebRequest.Create(url);

            var objStream = request.GetResponse().GetResponseStream();

            if (objStream != null)
            {
                var objReader = new StreamReader(objStream);
                var googleResults = objReader.ReadToEnd();

                var recaptchaResult = JsonConvert.DeserializeObject<ReCaptchaResponse>(googleResults);
                if (!recaptchaResult.Success)
                {
                    return false;
                }
            }
            return true;
        }

        #region Get phone code by country name
        [HttpGet]
        public JsonResult GetPhoneCodeByCountryCode(string countryCode)
        {
            string phoneCode = string.Empty;
            try
            {
                var countries = SiteSettingsHandler.Instance.SiteSettings.ListCountry;
                var listCountry = JsonConvert.DeserializeObject<Countries>(countries);
                var country = listCountry.countries.Where(x => x.code == countryCode).FirstOrDefault();
                if (listCountry != null)
                    phoneCode = country.dial_code;

                return Json(new { phone = phoneCode }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new {phone = phoneCode}, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion
        #region Get all country
        [OutputCache(Duration = 100000, VaryByParam = "none")]
        protected string GetAllCountries()
        {
            string htmlResult = string.Empty;
            try
            {
                var countries = SiteSettingsHandler.Instance.SiteSettings.ListCountry;
                var listCountry = JsonConvert.DeserializeObject<Countries>(countries);
                if (listCountry != null && listCountry.countries.Any())
                {
                    foreach (var country in listCountry.countries)
                    {
                        htmlResult += "<option value='" + country.code + "'>" + country.name + "</option>";
                    }
                }
                return  htmlResult ;
            }
            catch (Exception)
            {
                return string.Empty ;
            }
        }
    }
        #endregion
    #region Class for parse data 
    public class Countries
    {
        public List<Country> countries;
    }
    public class Country
    {
        public string code;
        public string name;
        public string dial_code;
    }
    public class ReCaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
#endregion

}
