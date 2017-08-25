using System;
using System.Configuration;
namespace Niteco.Common.Search.Configuration
{
	public class SearchResultFilterElement : ConfigurationElement
	{
		[ConfigurationProperty("defaultInclude", IsRequired = true, DefaultValue = false)]
		public bool SearchResultFilterDefaultInclude
		{
			get
			{
				return (bool)base["defaultInclude"];
			}
			set
			{
				base["defaultInclude"] = value;
			}
		}
		[ConfigurationProperty("providers", IsRequired = false)]
		public ProviderSettingsCollection Providers
		{
			get
			{
				return (ProviderSettingsCollection)base["providers"];
			}
		}
	}
}
