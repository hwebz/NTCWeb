using System;
using System.Web;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Web;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace Niteco.Cdn
{
    [TemplateDescriptor(Inherited = true, TemplateTypeCategory = TemplateTypeCategories.HttpHandler)]
    public class CachePolicyHttpHandler : BlobHttpHandler, IRenderTemplate<ImageData>
    {
        protected override void SetCachePolicy(HttpContextBase context, DateTime fileChangedDate)
        {
            var isCdnRequest = HttpContext.Current.Items[CdnConfigurationSection.GetConfiguration().ItemKey];

            if (isCdnRequest == null || !(bool)isCdnRequest)
            {
                base.SetCachePolicy(context, fileChangedDate);
                return;
            }

            fileChangedDate = fileChangedDate > DateTime.UtcNow ? DateTime.UtcNow : fileChangedDate;

            context.Response.Cache.SetLastModified(fileChangedDate);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
            context.Response.Cache.SetValidUntilExpires(true);
            context.Response.Cache.VaryByParams.IgnoreParams = true;
            context.Response.Cache.SetOmitVaryStar(true);
            context.Response.Cache.SetNoServerCaching();
        }

        protected override Blob GetBlob(HttpContextBase httpContext)
        {
            var customRouteData = httpContext.Request.RequestContext.GetCustomRouteData<string>(DownloadMediaRouter.DownloadSegment);
            if (!string.IsNullOrEmpty(customRouteData))
            {
                 httpContext.Response.AppendHeader("Content-Disposition", string.Format((string) "attachment; filename=\"{0}\"", (object) customRouteData));
            }
            var binaryStorable = ServiceLocator.Current.GetInstance<ContentRouteHelper>().Content as IBinaryStorable;
            return binaryStorable == null ? null : binaryStorable.BinaryData;
        }
    }
}