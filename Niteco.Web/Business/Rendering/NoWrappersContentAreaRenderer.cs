using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Framework.Web;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;

namespace Niteco.Web.Business.Rendering
{
    /// <summary>
    /// Renders blocks without wrapping (div) tags.
    /// </summary>
    public class NoWrappersContentAreaRenderer
    {
        private readonly IContentRenderer _contentRenderer;
        private readonly TemplateResolver _templateResolver;
        private readonly ContentRequestContext _contentRequestContext;

        public NoWrappersContentAreaRenderer(IContentRenderer contentRenderer,
            TemplateResolver templateResolver,
            ContentRequestContext contentRequestContext)
        {
            _contentRenderer = contentRenderer;
            _templateResolver = templateResolver;
            _contentRequestContext = contentRequestContext;
        }

        public virtual void Render(HtmlHelper helper, ContentArea contentArea)
        {
            var contents = (_contentRequestContext.IsInEditMode(helper.ViewContext.HttpContext)
                                ? contentArea.Contents
                                : contentArea.FilteredContents).ToArray();

            foreach (var content in contents)
            {
                var templateModel = _templateResolver.Resolve(helper.ViewContext.HttpContext,
                    content.GetOriginalType(), content,
                    TemplateTypeCategories.MvcPartial, null);

                using (new ContentAreaContext(helper.ViewContext.RequestContext, content.ContentLink))
                {
                    helper.RenderContentData(content, true, templateModel, _contentRenderer);
                }
            }
        }
    }

    public class ContentRequestContext
    {
        public virtual bool IsInEditMode(HttpContextBase httpContext)
        {
            return PageEditing.PageIsInEditMode;
        }
    }
}