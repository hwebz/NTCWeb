using System.Configuration;

namespace Niteco.Cdn
{
    public class CdnConfigurationSection : ConfigurationSection
    {
        public const string ConfigurationSectionName = "niteco.cdn";

        public const string EnabledAttributeName = "enabled";

        public const string UrlAttributeName = "url";

        public const string CacheKeyAttributeName = "cacheKey";

        public const string AdminPathAttributeName = "adminPath";

        [ConfigurationProperty(EnabledAttributeName, IsRequired = false, DefaultValue = "true")]
        public bool Enabled
        {
            get { return (bool)base[EnabledAttributeName]; }
        }

        [ConfigurationProperty(UrlAttributeName, IsRequired = false, DefaultValue = "")]
        public string Url
        {
            get { return (string)base[UrlAttributeName]; }
        }

        [ConfigurationProperty(CacheKeyAttributeName, IsRequired = false, DefaultValue = "cdn-image-cache")]
        public string ItemKey
        {
            get { return (string)base[CacheKeyAttributeName]; }
        }

        [ConfigurationProperty(AdminPathAttributeName, IsRequired = false, DefaultValue = "manage")]
        public string AdminPath
        {
            get { return (string)base[AdminPathAttributeName]; }
        }

        public static CdnConfigurationSection GetConfiguration()
        {
            var configuration = ConfigurationManager.GetSection(ConfigurationSectionName) as CdnConfigurationSection;
            return configuration ?? new CdnConfigurationSection();
        }
    }
}