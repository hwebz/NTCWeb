using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Niteco.Common.Extensions;


namespace Niteco.Web.Helpers
{
    public class CategoryHelper
    {
        public static List<Category> GetCategories(string categoryRootPath)
        {
            var catRepo = ServiceLocator.Current.GetInstance<CategoryRepository>();
            var catRoot = catRepo.GetRoot().GetCategoryByPath(categoryRootPath);

            if (catRoot == null)
                return new List<Category>();
            //Get sub categories
            return catRoot.Categories.ToList();
        }

        public static Category GetCategory(string categoryId)
        {
            int id;
            if (!string.IsNullOrEmpty(categoryId) && int.TryParse(categoryId, out id))
            {
                var categoryRepository = ServiceLocator.Current.GetInstance<CategoryRepository>();
                return categoryRepository.Get(id);
            }
            return null;
        }
        public static Category GetCategoryByName(string categoryName)
        {
            if (!string.IsNullOrEmpty(categoryName))
            {
                var categoryRepository = ServiceLocator.Current.GetInstance<CategoryRepository>();
                return categoryRepository.Get(categoryName);
            }
            return null;
        }
        public static Category GetCategoryByName(string categoryName, string categoryRootName)
        {
            if (!string.IsNullOrEmpty(categoryName) && !string.IsNullOrEmpty(categoryRootName))
            {
                var categoryRepository = ServiceLocator.Current.GetInstance<CategoryRepository>();
                var root = categoryRepository.Get(categoryRootName);
                if (root != null)
                    return root.FindChild(categoryName);
            }
            return null;
        }
    }
}