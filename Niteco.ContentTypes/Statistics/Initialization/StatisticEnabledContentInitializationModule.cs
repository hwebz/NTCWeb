using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer;
using EPiServer.Framework.Cache;
using Niteco.ContentTypes.Statistics.Cache;
using Niteco.ContentTypes.Statistics.Interfaces;

namespace Niteco.ContentTypes.Statistics.Initialization
{
    [InitializableModule, ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class StatisticEnabledContentInitializationModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.PublishingContent += ContentEvents_ChangingContent;
            contentEvents.DeletingContent += ContentEvents_ChangingContent;
        }

        private void ContentEvents_ChangingContent(object sender, ContentEventArgs e)
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            PageData pageData;
            if (!contentLoader.TryGet(e.ContentLink, out pageData))
            {
                return;
            }
            var synchronizedObjectInstanceCache = ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>();
            if (pageData is IHaveTag)
            {
                synchronizedObjectInstanceCache.Remove(TagStatisticServiceDecorate.CacheKey);
            }
            if (pageData is IHaveAuthor)
            {
                synchronizedObjectInstanceCache.Remove(AuthorStatisticServiceDecorate.CacheKey);
            }
            if (pageData is IHaveCategory)
            {
                synchronizedObjectInstanceCache.Remove(CategoryStatisticServiceDecorate.CacheKey);
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.PublishingContent -= ContentEvents_ChangingContent;
            contentEvents.DeletingContent -= ContentEvents_ChangingContent;
        }
    }
}