using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using Niteco.ContentTypes.Statistics.Models;
using Niteco.ContentTypes.Statistics.Services;

namespace Niteco.ContentTypes.Statistics.Cache
{
    public class TagStatisticServiceDecorate : ITagStatisticService
    {
        private readonly ITagStatisticService _tagStatisticService;
        private readonly ISynchronizedObjectInstanceCache _synchronizedObjectInstanceCache;
        public const string CacheKey = "TagStatisticServiceCache";
        public TagStatisticServiceDecorate(ITagStatisticService tagStatisticService, ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache)
        {
            _tagStatisticService = tagStatisticService;
            _synchronizedObjectInstanceCache = synchronizedObjectInstanceCache;
        }
        public IEnumerable<IStatisticItem> GetStatisticItems(ContentReference rootContentReference)
        {
            var statisticItems = _synchronizedObjectInstanceCache.Get(CacheKey) as IEnumerable<IStatisticItem>;
            if (statisticItems != null)
                return statisticItems;
            statisticItems = _tagStatisticService.GetStatisticItems(rootContentReference);
            _synchronizedObjectInstanceCache.Insert(CacheKey, statisticItems, new CacheEvictionPolicy(new TimeSpan(0, 30, 0), CacheTimeoutType.Absolute));
            return statisticItems;
        }
    }
}
