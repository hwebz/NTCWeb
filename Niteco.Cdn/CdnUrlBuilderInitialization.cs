using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Security;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace Niteco.Cdn
{
    [InitializableModule]
    public class CdnUrlBuilderInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            ContentRoute.CreatedVirtualPath += CreatedVirtualPath;
        }

        private void CreatedVirtualPath(object sender, UrlBuilderEventArgs urlBuilderEventArgs)
        {
            var cdnConfiguration = CdnConfigurationSection.GetConfiguration();

            var originalPath = urlBuilderEventArgs.UrlBuilder.Path;

            if (originalPath.StartsWith(cdnConfiguration.AdminPath))
            {
                return;
            }

            var enabled = cdnConfiguration.Enabled;
            if (!enabled)
            {
                return;
            }

            object contentReferenceObject;
            if (!urlBuilderEventArgs.RouteValues.TryGetValue(RoutingConstants.NodeKey, out contentReferenceObject))
            {
                return;
            }

            var routedContentLink = contentReferenceObject as ContentReference;
            if (ContentReference.IsNullOrEmpty(routedContentLink))
            {
                return;
            }

            // Check content type is Image or not
            var imageData = DataFactory.Instance.Get<IContent>(routedContentLink) as ImageData;
            if (imageData == null)
            {
                return;
            }

            // Check access rights
            var securable = imageData as ISecurable;
            if ((securable.GetSecurityDescriptor().GetAccessLevel(PrincipalInfo.AnonymousPrincipal) & AccessLevel.Read) != AccessLevel.Read)
            {
                return;
            }

            var baseUrl = CdnConfigurationSection.GetConfiguration().Url;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = SiteDefinition.Current.SiteUrl.AbsoluteUri;
            }

            urlBuilderEventArgs.UrlBuilder.Uri = new Uri(string.Format("{0}/{1}", baseUrl.TrimEnd('/'), originalPath.TrimStart('/')));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}
