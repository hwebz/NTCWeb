using System;
using System.Configuration;
namespace Niteco.Common.Search.Configuration
{
	public class NamedIndexingServicesElement : ConfigurationElement
	{
		[ConfigurationProperty("defaultService", IsRequired = true)]
		public string DefaultService
		{
			get
			{
				return (string)base["defaultService"];
			}
			set
			{
				base["defaultService"] = value;
			}
		}
		[ConfigurationProperty("services", IsRequired = true)]
		public NamedIndexingServiceCollection NamedIndexingServices
		{
			get
			{
				return (NamedIndexingServiceCollection)base["services"];
			}
		}
	}
}
