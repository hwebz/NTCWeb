using System;
using System.Net;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using Niteco.ContentTypes;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Models.ViewModels;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EPiServer.Logging.Compatibility;
using System.Configuration;
using System.IO;
using System.Linq;
using EPiServer.DataAccess;
using EPiServer.Editor;
using EPiServer.Security;
using Newtonsoft.Json;
using WebMarkupMin.AspNet4.Mvc;

namespace Niteco.Web.Controllers
{
    public class NewsDetailPageController : PageControllerBase<NewsDetailPage>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NewsDetailPageController));
        private readonly IContentLoader _contentLoader;
        private readonly IContentRepository _contentRepository;

        public NewsDetailPageController(IContentLoader contentLoader, IContentRepository contentRepository)
        {
            _contentLoader = contentLoader;
            _contentRepository = contentRepository;
        }

        [MinifyHtml]
        public ActionResult Index(NewsDetailPage currentPage)
        {
            ViewBag.SubscriptionText = SiteSettingsHandler.Instance.SiteSettings.SubscriptionText + "";
            ViewBag.SubscriptionDescription = SiteSettingsHandler.Instance.SiteSettings.SubscriptionDescription + "";
            ViewBag.MailChimpApiKey = SiteSettingsHandler.Instance.SiteSettings.MailChimpApiKey + "";
            ViewBag.MailChimpListId = SiteSettingsHandler.Instance.SiteSettings.MailChimpListId + "";
            ViewBag.SubscriptionUrlPost = SiteSettingsHandler.Instance.SiteSettings.SubcriptionUrlPost + "";
            var newsDetailViewModel = new NewsDetailViewModel(currentPage)
            {
                RelatedNewsDetailPages =
                    _contentLoader.GetChildren<NewsDetailPage>(currentPage.ParentLink)
                        .Where(
                            x =>
                                x != currentPage && DateTime.Compare(x.StartPublish, DateTime.Now) < 0 &&
                                DateTime.Compare(x.StopPublish, DateTime.Now) > 0)
                        .OrderByDescending(x => x.StartPublish)
                        .Take(3)
            };
            
            return View(newsDetailViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Subscribe(string email)
        {
            HttpClient httpClient = null;
            HttpRequestMessage httpRequestMessage = null;
            HttpResponseMessage httpResponseMessage = null;
            try
            {
                var result = Request.Form["g-recaptcha-response"];
                var url = string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", ConfigurationManager.AppSettings["ReCaptchaSiteSecret"], result);
                var request = WebRequest.Create(url);
                var webResponse = await request.GetResponseAsync();
                var objStream = webResponse.GetResponseStream();
                if (objStream != null)
                {
                    var objReader = new StreamReader(objStream);
                    var googleResults = objReader.ReadToEnd();

                    var recaptchaResult = JsonConvert.DeserializeObject<ReCaptchaResponse>(googleResults);
                    if (!recaptchaResult.Success)
                    {
                        throw new Exception("Invalid captcha");
                    }
                }
                var apiKey = SiteSettingsHandler.Instance.SiteSettings.MailChimpApiKey;
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("Missing mail chimp api key");
                }
                var subDomain = apiKey.Substring(apiKey.LastIndexOf("-", StringComparison.Ordinal) + 1);
                if (string.IsNullOrEmpty(subDomain))
                {
                    throw new Exception("Invalid mail chimp api key");
                }
                var listId = SiteSettingsHandler.Instance.SiteSettings.MailChimpListId;
                if (string.IsNullOrEmpty(listId))
                {
                    throw new Exception("Empty mail chimp list id");
                }
                httpClient = new HttpClient { BaseAddress = new Uri(string.Format("https://{0}.api.mailchimp.com", subDomain)), Timeout = new TimeSpan(0, 0, 30) };
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format("3.0/lists/{0}/members", listId));
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("apikey", apiKey);
                httpRequestMessage.Content = new StringContent("{\"email_address\":\"" + email + "\",\"status\":\"subscribed\"}");
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                throw new Exception(await httpResponseMessage.Content.ReadAsStringAsync());
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return RedirectToAction("Index");
            }
            finally
            {
                if (httpClient != null)
                {
                    httpClient.Dispose();
                }
                if (httpRequestMessage != null)
                {
                    httpRequestMessage.Dispose();
                }
                if (httpResponseMessage != null)
                {
                    httpResponseMessage.Dispose();
                }
            }
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            // don't throw 404 in edit mode
            if (!PageEditing.PageIsInEditMode)
            {
                if (PageContext.Page.StopPublish <= DateTime.Now)
                {
                    filterContext.Result = new HttpStatusCodeResult(404, "Not found");
                    return;
                }
            }

            base.OnAuthorization(filterContext);
        }
    }
}