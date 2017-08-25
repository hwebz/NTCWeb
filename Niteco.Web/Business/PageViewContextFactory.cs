using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using EPiServer.Shell.ViewComposition;
using Niteco.ContentTypes;
using Niteco.ContentTypes.Pages;
using Niteco.Web.Helpers;
using Niteco.Web.Models.ViewModels;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace Niteco.Web.Business
{
    public class PageViewContextFactory
    {
        private readonly IContentLoader _contentLoader;
        private readonly UrlResolver _urlResolver;
        public PageViewContextFactory(IContentLoader contentLoader, UrlResolver urlResolver)
        {
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
        }

        public virtual LayoutModel CreateLayoutModel(ContentReference currentContentLink, RequestContext requestContext)
        {
            var startPage = _contentLoader.Get<StartPage>(SiteDefinition.Current.StartPage);
            var settingPage = SiteSettingsHandler.Instance.SiteSettings;
            //var companyPages = _contentLoader.GetChildren<PageData>(startPage.CompanyInformationPageLink)
            //    .FilterForDisplay(requirePageTemplate: true)
            //    .ToList();
            //companyPages.Insert(0, _contentLoader.Get<PageData>(startPage.CompanyInformationPageLink));
            var md = new LayoutModel
                {
                    LogotypeLinkUrl = new MvcHtmlString(_urlResolver.GetUrl(SiteDefinition.Current.StartPage)),
                    LoggedIn = requestContext.HttpContext.User.Identity.IsAuthenticated,
                    LoginUrl = new MvcHtmlString(GetLoginUrl(currentContentLink)),
                    GlobalBodyScripts = settingPage != null ? settingPage.GlobalBodyScripts : string.Empty,
                    GlobalFooterScripts = settingPage != null ? settingPage.GlobalFooterScripts : string.Empty,
                    GlobalHeadScripts = settingPage != null ? settingPage.GlobalHeadScripts : string.Empty
                };
            if (settingPage != null)
            {
                md.LogoImage = settingPage.LogoImage;
                md.Slogan = settingPage.Slogan;
                md.LinkedinLink = settingPage.LinkedinLink;
                md.FacebookLink = settingPage.FacebookLink;
                md.FooterItemsContent = settingPage.FooterItemsContent;
                md.Email = settingPage.Email;
                md.SearchPageLink = settingPage.SearchPageLink;
                md.AssetsVersion = settingPage.AssetsVersion;
            }
            return md;
        }

        private string GetLoginUrl(ContentReference returnToContentLink)
        {
            return string.Format(
                "{0}?ReturnUrl={1}", 
                FormsAuthentication.LoginUrl,
                _urlResolver.GetUrl(returnToContentLink));
        }

        public virtual IContent GetSection(ContentReference contentLink)
        {
            var currentContent = _contentLoader.Get<IContent>(contentLink);
            if (currentContent.ParentLink != null && currentContent.ParentLink.CompareToIgnoreWorkID(SiteDefinition.Current.StartPage))
            {
                return currentContent;
            }

            return _contentLoader.GetAncestors(contentLink)
                .OfType<PageData>()
                .SkipWhile(x => x.ParentLink == null || !x.ParentLink.CompareToIgnoreWorkID(SiteDefinition.Current.StartPage))
                .FirstOrDefault();
        }
    }
}
