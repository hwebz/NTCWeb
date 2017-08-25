using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using EPiServer;
using EPiServer.Core;
using EPiServer.SpecializedProperties;
using Niteco.ContentTypes.Blocks;

namespace Niteco.Web.Models.ViewModels
{
    public class LayoutModel
    {
        public Url LogoImage { get; set; }
        public string Slogan { get; set; }
        public Url FacebookLink { get; set; }
        public Url LinkedinLink { get; set; }
        public Url Email { get; set; }
        public IHtmlString LogotypeLinkUrl { get; set; }
        public bool LoggedIn { get; set; }
        public MvcHtmlString LoginUrl { get; set; }
        public MvcHtmlString LogOutUrl { get; set; }
        public ContentArea FooterItemsContent { get; set; }

        public ContentReference SearchPageLink { get; set; }

        public string GlobalHeadScripts { get; set; }
        public string GlobalBodyScripts { get; set; }
        public string GlobalFooterScripts { get; set; }

        public string AssetsVersion { get; set; }
    }
}
