using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using Niteco.ContentTypes.Pages;
using Niteco.ContentTypes.Statistics.Interfaces;
using Niteco.ContentTypes.Statistics.Models;
using Niteco.ContentTypes.Statistics.Services;

namespace Niteco.ContentTypes.Statistics.Implementations
{
    public class AuthorStatisticService : IAuthorStatisticService
    {
        private readonly IContentLoader _contentLoader;

        public AuthorStatisticService(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        public IEnumerable<IStatisticItem> GetStatisticItems(ContentReference rootContentReference)
        {
            var pageDatas = _contentLoader.GetChildren<IHaveAuthor>(rootContentReference);
            var authorContainer = SiteSettingsHandler.Instance.SiteSettings.AuthorContainer;
            if (ContentReference.IsNullOrEmpty(authorContainer))
            {
                return new List<IStatisticItem>();
            }
            var authors = _contentLoader.GetChildren<IHaveAuthor>(authorContainer);
            var statisticItems = (from author in authors
                                  let count = pageDatas.Count(pageData => pageData.Author != null && pageData.Author.Equals(author.Author, true))
                                  select new StatisticItem
                                  {
                                      Name = _contentLoader.Get<AuthorPage>(author.Author).AuthorName,
                                      Count = count
                                  }).ToList();
            return statisticItems;
        }
    }
}
