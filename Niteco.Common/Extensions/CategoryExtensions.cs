using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace Niteco.Common.Extensions
{
    public static class CategoryExtensions
    {
        public static Category GetCategoryByPath(this Category catRoot, string catPath)
        {
            Category result = null;
            if (!string.IsNullOrEmpty(catPath))
            {
                var catNames = catPath.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                var catCount = catNames.Length;
                var i = 0;

                while (catRoot != null && i < catCount)
                {
                    catRoot = FindChildWithoutRecusive(catRoot, catNames[i]);
                    i++;
                }

                if (catRoot != null && catRoot.Name.Equals(catNames[catCount - 1], StringComparison.InvariantCultureIgnoreCase))
                {
                    result = catRoot;
                }
            }

            return result;
        }

        private static Category FindChildWithoutRecusive(Category catRoot, string catName)
        {
            if (catRoot.Categories.Count == 0)
                return null;

            var category = catRoot.Categories.FirstOrDefault(x => string.Equals(x.Name, catName, StringComparison.InvariantCultureIgnoreCase));

            return category;
        }
        public static List<Category> GetCategoryList(this CategoryList list)
        {

            if (list != null && list.Any())
            {
                var catRepo = ServiceLocator.Current.GetInstance<CategoryRepository>();
                return list.Select(x => catRepo.Get(x)).ToList();
            }
            return new List<Category>();
        }
    }
}
