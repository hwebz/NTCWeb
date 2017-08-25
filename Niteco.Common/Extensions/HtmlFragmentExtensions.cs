using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using EPiServer.HtmlParsing;
using EPiServer.XForms;
using EPiServer.XForms.Util;

namespace Niteco.Common.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="HtmlHelper"/> class
    /// </summary>
    public static class HtmlFragmentExtensions
    {
        /// <summary>
        /// Writes the XForm to the view context's output steam.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="xform">The XForm to write.</param>
        public static void RenderCustomXForm(this HtmlHelper htmlHelper, XForm xform)
        {
            if (xform == null)
            {
                return;
            }

            htmlHelper.RenderCustomXForm(xform, new XFormParameters());
        }

        /// <summary>
        /// Writes the XForm to the view context's output steam.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="xform">The XForm to write.</param>
        /// <param name="parameters">The parameters to be used by the XForm.</param>
        public static void RenderCustomXForm(this HtmlHelper htmlHelper, XForm xform, XFormParameters parameters)
        {
            if (xform == null)
            {
                return;
            }

            htmlHelper.ViewContext.ViewData["XFormParameters"] = parameters;

            htmlHelper.RenderPartial(typeof(XForm).Name, xform);
        }

        /// <summary>
        /// Renders the html fragment.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="fragment">The fragment.</param>
        /// <param name="renderFragmentsWithoutView">if set to <c>true</c> [render fragments without view].</param>
        /// <returns>A HTML-encoded string.</returns>
        public static MvcHtmlString RenderCustomHtmlFragment(this HtmlHelper htmlHelper, HtmlFragment fragment, bool renderFragmentsWithoutView = true)
        {
            var value = new StringBuilder();

            using (var writer = new StringWriter(value))
            {
                // Try to find a matching partial view
                var viewEngineResult = ViewEngines.Engines.FindPartialView(htmlHelper.ViewContext, fragment.GetType().Name);

                if (viewEngineResult.View != null)
                {
                    viewEngineResult.View.Render(viewEngineResult, htmlHelper.ViewContext, writer, fragment);
                }
                else if (renderFragmentsWithoutView)
                {
                    // Render fragment directly unless otherwise specified
                    writer.Write(fragment.ToString());
                }
            }

            return value.Length <= 0 ? MvcHtmlString.Empty : new MvcHtmlString(value.ToString().Trim());
        }

        /// <summary>
        /// Renders the HTML fragment.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="fragment">The fragment.</param>
        /// <param name="partialViewName">The partial view name.</param>
        /// <param name="useFallback">if set to <c>true</c> [use fallback].</param>
        /// <param name="renderFragmentsWithoutView">if set to <c>true</c> [render fragments without view].</param>
        /// <returns>A HTML-encoded string.</returns>
        public static MvcHtmlString RenderCustomHtmlFragment(this HtmlHelper htmlHelper, HtmlFragment fragment, string partialViewName, bool useFallback = true, bool renderFragmentsWithoutView = true)
        {
            var value = new StringBuilder();

            using (var writer = new StringWriter(value))
            {
                // Try to find a matching partial view
                var viewEngineResult = ViewEngines.Engines.FindPartialView(htmlHelper.ViewContext, partialViewName);

                if (viewEngineResult.View != null)
                {
                    viewEngineResult.View.Render(viewEngineResult, htmlHelper.ViewContext, writer, fragment);
                }
                else
                {
                    // Try to fall back to default view
                    if (useFallback)
                    {
                        return htmlHelper.RenderCustomHtmlFragment(fragment, renderFragmentsWithoutView);
                    }

                    // Render fragment directly unless otherwise specified
                    if (renderFragmentsWithoutView)
                    {
                        writer.Write(fragment.ToString());
                    }
                }
            }

            return value.Length <= 0 ? MvcHtmlString.Empty : new MvcHtmlString(value.ToString().Trim());
        }

        /// <summary>
        /// Renders the HTML fragment.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="viewEngineResult">The view engine result.</param>
        /// <param name="context">The context.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="fragment">The fragment.</param>
        private static void Render(this IView view, ViewEngineResult viewEngineResult, ViewContext context, TextWriter writer, HtmlFragment fragment)
        {
            var viewDataDictionaries = new ViewDataDictionary(fragment);

            foreach (var viewDatum in context.ViewData)
            {
                viewDataDictionaries[viewDatum.Key] = viewDatum.Value;
            }

            var viewContext = new ViewContext(context, viewEngineResult.View, viewDataDictionaries, context.TempData, writer);

            foreach (var modelState in context.ViewData.ModelState)
            {
                viewContext.ViewData.ModelState.Add(modelState.Key, modelState.Value);
            }

            viewEngineResult.View.Render(viewContext, writer);
            viewEngineResult.ViewEngine.ReleaseView(context.Controller.ControllerContext, viewEngineResult.View);
        }
    }
}
