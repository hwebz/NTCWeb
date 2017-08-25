using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class PageEntityInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var eventRegistry = ServiceLocator.Current.GetInstance<IContentEvents>();
            eventRegistry.CreatingContent += OnCreatingContent_News_BlogPage;
            eventRegistry.PublishingContent += OnPublishedContent;
        }

        private void OnPublishedContent(object sender, ContentEventArgs e)
        {
            if (e.Content is BlogListingPage || e.Content is NewsListingPage)
            {
                var currentNode = e.Content as SitePageData;
                var repository = ServiceLocator.Current.GetInstance<IContentRepository>();
                var referenceLinkList = repository.GetDescendents(e.Content.ContentLink);
                foreach (var item in referenceLinkList)
                {
                    SitePageData page = null;
                    repository.TryGet(item, out page);
                    if (page != null)
                    {
                        var pageTemp = (SitePageData)page.CreateWritableClone();
                        pageTemp.Heading = currentNode.Heading;
                        pageTemp.BackgroundImage = currentNode.BackgroundImage;
                        pageTemp.MediumBackgroundUrl = currentNode.MediumBackgroundUrl;
                        pageTemp.SmallBackgroundUrl = currentNode.SmallBackgroundUrl;
                        repository.Save(pageTemp, EPiServer.DataAccess.SaveAction.Publish);
                    }
                }
            }
        }

        private void OnCreatingContent_News_BlogPage(object sender, ContentEventArgs e)
        {
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var currentPage = e.Content as SitePageData;


            if (currentPage != null)
            {
                var parentLink = currentPage != null ? currentPage.ParentLink : null;
                if (parentLink == null) return;
                SitePageData parentPage;
                if (contentRepository.TryGet(parentLink, out parentPage))
                {
                    if (parentPage is NewsListingPage || parentPage is BlogListingPage)
                    {
                        currentPage.BackgroundImage = parentPage.BackgroundImage;
                        currentPage.MediumBackgroundUrl = parentPage.MediumBackgroundUrl;
                        currentPage.SmallBackgroundUrl = parentPage.SmallBackgroundUrl;
                        currentPage.Heading = parentPage.Heading;
                    }
                }
            }


        }

        public void Uninitialize(InitializationEngine context)
        {
            var eventRegistry = ServiceLocator.Current.GetInstance<IContentEvents>();
            eventRegistry.CreatingContent -= OnCreatingContent_News_BlogPage;
            eventRegistry.PublishedContent -= OnPublishedContent;
        }
    }
}