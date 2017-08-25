using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using Niteco.ContentTypes.Statistics.Interfaces;
using Niteco.ContentTypes.Statistics.Models;
using Niteco.ContentTypes.Statistics.Services;

namespace Niteco.ContentTypes.Statistics.Implementations
{
    public class CategoryStatisticService : ICategoryStatisticService
    {
        private readonly IContentLoader _contentLoader;
        private readonly CategoryRepository _categoryRepository;
        public CategoryStatisticService(IContentLoader contentLoader, CategoryRepository categoryRepository)
        {
            _contentLoader = contentLoader;
            _categoryRepository = categoryRepository;
        }

        public IEnumerable<IStatisticItem> GetStatisticItems(ContentReference rootContentReference)
        {
            var rootBlogCategory = _categoryRepository.Get("Blog Category");
            if (rootBlogCategory == null)
            {
                return new List<IStatisticItem>();
            }
            var categories = rootBlogCategory.Categories.Select(category => category.Name).ToList();
            var pageDatas = _contentLoader.GetChildren<IHaveCategory>(rootContentReference);
            return (from category in categories
                    let count = pageDatas.Count(pageData => string.Equals(pageData.BlogCategory, category, StringComparison.InvariantCultureIgnoreCase))
                    select new StatisticItem
                    {
                        Name = category,
                        Count = count
                    }).ToList();
        }
    }
}
