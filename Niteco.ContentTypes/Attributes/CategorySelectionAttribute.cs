using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.DataAbstraction;

namespace Niteco.ContentTypes.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CategorySelectionAttribute : Attribute
    {
        /// 
        ///  ID of the root category.
        /// 
        public int RootCategoryId { get; set; }

        /// 
        /// Name of the root category.
        /// 
        public string RootCategoryName { get; set; }

        /// 
        /// The appSetting key containing the root category id to use.
        /// 
        public string RootCategoryAppSettingKey { get; set; }

        public int GetRootCategoryId()
        {
            if (RootCategoryId > 0)
            {
                return RootCategoryId;
            }

            if (!string.IsNullOrWhiteSpace(RootCategoryName))
            {
                var category = Category.Find(RootCategoryName);

                if (category != null)
                {
                    return category.ID;
                }
            }

            if (!string.IsNullOrWhiteSpace(RootCategoryAppSettingKey))
            {
                string appSettingValue = ConfigurationManager.AppSettings[RootCategoryAppSettingKey];
                int rootCategoryId;

                if (!string.IsNullOrWhiteSpace(appSettingValue) && int.TryParse(appSettingValue, out rootCategoryId))
                {
                    return rootCategoryId;
                }
            }

            return Category.GetRoot().ID;
        }
    }
}
