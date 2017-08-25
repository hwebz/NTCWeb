using System.Configuration;

namespace Niteco.Search.IndexingService.Configuration
{
	public class NamedIndexesElement : ConfigurationElement
	{
		[ConfigurationProperty("defaultIndex", IsRequired = true)]
		public string DefaultIndex
		{
			get
			{
				return (string)base["defaultIndex"];
			}
			set
			{
				base["defaultIndex"] = value;
			}
		}
		[ConfigurationProperty("indexes", IsRequired = true)]
		public NamedIndexCollection NamedIndexes
		{
			get
			{
				return (NamedIndexCollection)base["indexes"];
			}
		}
	}
}
