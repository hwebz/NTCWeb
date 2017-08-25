using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using Niteco.ContentTypes.Statistics.Interfaces;
using Niteco.ContentTypes.Statistics.Models;
using Niteco.ContentTypes.Statistics.Services;
using Niteco.UI.Tags.Interfaces;

namespace Niteco.ContentTypes.Statistics.Implementations
{
    public class TagStatisticService : ITagStatisticService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IContentLoader _contentLoader;

        public TagStatisticService(ITagRepository tagRepository, IContentLoader contentLoader)
        {
            _tagRepository = tagRepository;
            _contentLoader = contentLoader;
        }

        public IEnumerable<IStatisticItem> GetStatisticItems(ContentReference rootContentReference)
        {
            var pageDatas = _contentLoader.GetChildren<IHaveTag>(rootContentReference);
            var tags = _tagRepository.GetAllTags();
            var statisticItems = new List<StatisticItem>();
            if (tags == null)
            {
                return statisticItems;
            }
            foreach (var tag in tags)
            {
                var count = (from pageData in pageDatas where !string.IsNullOrEmpty(pageData.Tags) select pageData.Tags.Split(',')).Count(pageDataTags => pageDataTags.Any(x => string.Equals(x, tag.Name, StringComparison.InvariantCultureIgnoreCase)));
                statisticItems.Add(new StatisticItem
                {
                    Name = tag.Name,
                    Count = count
                });
            }
            return statisticItems;
        }
    }
}
