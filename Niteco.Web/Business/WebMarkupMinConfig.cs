using System.Collections.Generic;
using WebMarkupMin.AspNet.Common.UrlMatchers;
using WebMarkupMin.AspNet4.Common;
using WebMarkupMin.MsAjax;
using WebMarkupMin.Yui;

namespace Niteco.Web.Business
{
    public class WebMarkupMinConfig
    {
        public static void Configure(WebMarkupMinConfiguration configuration)
        {
            configuration.AllowMinificationInDebugMode = true;
            configuration.AllowCompressionInDebugMode = true;

            var htmlMinificationManager =
                HtmlMinificationManager.Current;
            htmlMinificationManager.ExcludedPages = new List<IUrlMatcher>
            {
                new WildcardUrlMatcher("/minifiers/x*ml-minifier"),
                new ExactUrlMatcher("/contact")
            };
            var htmlMinificationSettings =
                htmlMinificationManager.MinificationSettings;
            htmlMinificationSettings.RemoveRedundantAttributes = true;
            htmlMinificationSettings.RemoveHttpProtocolFromAttributes = true;
            htmlMinificationSettings.RemoveHttpsProtocolFromAttributes = true;
            htmlMinificationManager.CssMinifierFactory =
                new MsAjaxCssMinifierFactory();
            htmlMinificationManager.JsMinifierFactory =
                new MsAjaxJsMinifierFactory();

            var xhtmlMinificationManager =
                XhtmlMinificationManager.Current;
            xhtmlMinificationManager.IncludedPages = new List<IUrlMatcher>
            {
                new WildcardUrlMatcher("/minifiers/x*ml-minifier"),
                new ExactUrlMatcher("/contact")
            };
            var xhtmlMinificationSettings =
                xhtmlMinificationManager.MinificationSettings;
            xhtmlMinificationSettings.RemoveRedundantAttributes = true;
            xhtmlMinificationSettings.RemoveHttpProtocolFromAttributes = true;
            xhtmlMinificationSettings.RemoveHttpsProtocolFromAttributes = true;
            xhtmlMinificationManager.CssMinifierFactory =
                new YuiCssMinifierFactory();
            xhtmlMinificationManager.JsMinifierFactory =
                new YuiJsMinifierFactory();

            var xmlMinificationManager =
                XmlMinificationManager.Current;
            var xmlMinificationSettings =
                xmlMinificationManager.MinificationSettings;
            xmlMinificationSettings.CollapseTagsWithoutContent = true;
        }
    }
}