using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Niteco.Common.Consts;
using Niteco.ContentTypes.Pages;

namespace Niteco.ContentTypes
{
    /// <summary>
    /// Class SiteSettingsHandler. This class cannot be inherited.
    /// </summary>
    public sealed class SiteSettingsHandler
    {
        /// <summary>
        /// The _content repository
        /// </summary>
        private readonly IContentRepository _contentRepository;
        /// <summary>
        /// The _localization service
        /// </summary>
        private readonly LocalizationService _localizationService;

        #region singleton implementaion

        /// <summary>
        /// Prevents a default instance of the <see cref="SiteSettingsHandler"/> class from being created.
        /// </summary>
        private SiteSettingsHandler()
            : this(ServiceLocator.Current.GetInstance<IContentRepository>(), LocalizationService.Current)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSettingsHandler"/> class.
        /// </summary>
        /// <param name="contentRepository">The content repository.</param>
        /// <param name="localizationService">The localization service.</param>
        private SiteSettingsHandler(IContentRepository contentRepository, LocalizationService localizationService)
        {
            _contentRepository = contentRepository;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static SiteSettingsHandler Instance
        {
            get { return Singleton<SiteSettingsHandler>.Instance; }
        }

        #endregion

        /// <summary>
        /// Gets the site settings.
        /// </summary>
        /// <value>The site settings.</value>
        /// <exception cref="System.Configuration.ConfigurationErrorsException"></exception>
        public ISiteSettings SiteSettings
        {
            get
            {
                var cacheKey = SiteConst.SiteSettingCacheKey + SiteDefinition.Current.Name;
                var settings = CacheManager.Get(cacheKey);
                if (settings != null)
                {
                    return settings as ISiteSettings;
                }

                if (ContentReference.IsNullOrEmpty(SiteDefinition.Current.StartPage))
                {
                    return null;
                }
                // try to get setting from Setting Page
                var startPage = _contentRepository.Get<StartPage>(SiteDefinition.Current.StartPage);
                if (startPage.SettingPage == null || startPage.SettingPage.CompareToIgnoreWorkID(ContentReference.EmptyReference))
                {
                    throw new ConfigurationErrorsException(_localizationService.GetString("/niteco/error/missingsettingpage"));
                }
                var settingPage = _contentRepository.Get<SettingPageData>(startPage.SettingPage);

                // insert to cache with dependency to SettingPage so that whenever the SettingPage is published, cache will be invalidated
                var cacheEvictionPolicy = new CacheEvictionPolicy(new[] { DataFactoryCache.PageCommonCacheKey(startPage.SettingPage) });
                CacheManager.Insert(cacheKey, settingPage, cacheEvictionPolicy);

                return settingPage;
            }
        }
    }
}
