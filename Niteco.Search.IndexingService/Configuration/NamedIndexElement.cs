using System.Configuration;
using System.Web.Hosting;

namespace Niteco.Search.IndexingService.Configuration
{
	public class NamedIndexElement : ConfigurationElement
	{
		public const string AppDataPathKey = "[appDataPath]";
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name
		{
			get
			{
				return (string)base["name"];
			}
			set
			{
				base["name"] = value;
			}
		}
		[ConfigurationProperty("directoryPath", IsRequired = true, IsKey = true)]
		public string DirectoryPath
		{
			get
			{
				return (string)base["directoryPath"];
			}
			set
			{
				base["directoryPath"] = value;
			}
		}
		[ConfigurationProperty("readonly", IsRequired = false, DefaultValue = false)]
		public bool ReadOnly
		{
			get
			{
				return (bool)base["readonly"];
			}
			set
			{
				base["readonly"] = value;
			}
		}
		[ConfigurationProperty("pendingDeletesOptimizeThreshold", IsRequired = false, DefaultValue = 100)]
		public int PendingDeletesOptimizeThreshold
		{
			get
			{
				return (int)base["pendingDeletesOptimizeThreshold"];
			}
			set
			{
				base["pendingDeletesOptimizeThreshold"] = value;
			}
		}
		[ConfigurationProperty("idFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool IdFieldInResponse
		{
			get
			{
				return (bool)base["idFieldInResponse"];
			}
			set
			{
				base["idFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("uriFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool UriFieldInResponse
		{
			get
			{
				return (bool)base["uriFieldInResponse"];
			}
			set
			{
				base["uriFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("titleFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool TitleFieldInResponse
		{
			get
			{
				return (bool)base["titleFieldInResponse"];
			}
			set
			{
				base["titleFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("displayTextFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool DisplayTextFieldInResponse
		{
			get
			{
				return (bool)base["displayTextFieldInResponse"];
			}
			set
			{
				base["displayTextFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("createdFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool CreatedFieldInResponse
		{
			get
			{
				return (bool)base["createdFieldInResponse"];
			}
			set
			{
				base["createdFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("modifiedFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool ModifiedFieldInResponse
		{
			get
			{
				return (bool)base["modifiedFieldInResponse"];
			}
			set
			{
				base["modifiedFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("authorFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool AuthorFieldInResponse
		{
			get
			{
				return (bool)base["authorFieldInResponse"];
			}
			set
			{
				base["authorFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("categoriesFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool CategoriesFieldInResponse
		{
			get
			{
				return (bool)base["categoriesFieldInResponse"];
			}
			set
			{
				base["categoriesFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("aclFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool AclFieldInResponse
		{
			get
			{
				return (bool)base["aclFieldInResponse"];
			}
			set
			{
				base["aclFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("typeFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool TypeFieldInResponse
		{
			get
			{
				return (bool)base["typeFieldInResponse"];
			}
			set
			{
				base["typeFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("cultureFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool CultureFieldInResponse
		{
			get
			{
				return (bool)base["cultureFieldInResponse"];
			}
			set
			{
				base["cultureFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("virtualPathFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool VirtualPathFieldInResponse
		{
			get
			{
				return (bool)base["virtualPathFieldInResponse"];
			}
			set
			{
				base["virtualPathFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("publicationEndFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool PublicationEndFieldInResponse
		{
			get
			{
				return (bool)base["publicationEndFieldInResponse"];
			}
			set
			{
				base["publicationEndFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("publicationStartFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool PublicationStartFieldInResponse
		{
			get
			{
				return (bool)base["publicationStartFieldInResponse"];
			}
			set
			{
				base["publicationStartFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("metadataFieldInResponse", IsRequired = false, DefaultValue = false)]
		public bool MetadataFieldInResponse
		{
			get
			{
				return (bool)base["metadataFieldInResponse"];
			}
			set
			{
				base["metadataFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("referenceIdFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool ReferenceFieldInResponse
		{
			get
			{
				return (bool)base["referenceIdFieldInResponse"];
			}
			set
			{
				base["referenceIdFieldInResponse"] = value;
			}
		}
		[ConfigurationProperty("itemStatusFieldInResponse", IsRequired = false, DefaultValue = true)]
		public bool ItemStatusFieldInResponse
		{
			get
			{
				return (bool)base["itemStatusFieldInResponse"];
			}
			set
			{
				base["itemStatusFieldInResponse"] = value;
			}
		}
		public string GetDirectoryPath()
		{
			string text = this.DirectoryPath;
			if (text.StartsWith("[appDataPath]", System.StringComparison.OrdinalIgnoreCase))
			{
				string appDataBasePath = this.GetAppDataBasePath();
				if (string.IsNullOrEmpty(appDataBasePath))
				{
					throw new System.ArgumentException("Missing basePath attribute for the appData in the EPiServer Framework config");
				}
				text = System.IO.Path.Combine(appDataBasePath, text.Substring("[appDataPath]".Length).TrimStart(new char[]
				{
					'\\',
					'/'
				}));
			}
			text = System.Environment.ExpandEnvironmentVariables(text);
			if (!System.IO.Path.IsPathRooted(text))
			{
				text = System.IO.Path.Combine(HostingEnvironment.ApplicationPhysicalPath ?? System.AppDomain.CurrentDomain.BaseDirectory, text);
			}
			return text;
		}
		private string GetAppDataBasePath()
		{
			object section = ConfigurationManager.GetSection("episerver.framework");
			if (section != null)
			{
				object value = section.GetType().GetProperty("AppData").GetValue(section, null);
				if (value != null)
				{
					return value.GetType().GetProperty("BasePath").GetValue(value, null) as string;
				}
			}
			return null;
		}
	}
}
