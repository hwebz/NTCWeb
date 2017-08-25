using System.Configuration;
using Niteco.Common.Search.Custom;

namespace Niteco.Common.Search.Configuration
{
	public class SearchSection : ConfigurationSection
	{
		private static System.Configuration.Configuration _config;
		private static SearchSection _searchSection;
		private static object _syncObject = new object();
		private bool? _useIndexServicePaging = null;
		private int? _queueFlushInterval = null;
        public static System.Configuration.Configuration ConfigurationInstance
		{
			get
			{
				if (SearchSection._config != null)
				{
					return SearchSection._config;
				}
				lock (SearchSection._syncObject)
				{
					if (SearchSection._config == null)
					{
						SearchSection._config = SearchSection.GetConfigurationFromCurrentDomain();
					}
				}
				return SearchSection._config;
			}
			set
			{
				if (SearchSection._config == value)
				{
					return;
				}
				lock (SearchSection._syncObject)
				{
					SearchSection._config = value;
					SearchSection._searchSection = null;
				}
			}
		}
		public static SearchSection Instance
		{
			get
			{
				if (SearchSection._searchSection != null)
				{
					return SearchSection._searchSection;
				}
				lock (SearchSection._syncObject)
				{
					if (SearchSection._searchSection == null)
					{
                        SearchSection._searchSection = (SearchSection.ConfigurationInstance.GetSection(SearchConstants.SearchSectionName) as SearchSection);
					}
				}
				return SearchSection._searchSection;
			}
		}
		[ConfigurationProperty("active", IsRequired = true)]
		public bool Active
		{
			get
			{
				return (bool)base["active"];
			}
			set
			{
				base["active"] = value;
			}
		}
		[ConfigurationProperty("queueFlushInterval", IsRequired = false, DefaultValue = 30)]
		public int QueueFlushInterval
		{
			get
			{
				int? queueFlushInterval = this._queueFlushInterval;
				if (!queueFlushInterval.HasValue)
				{
					return (int)base["queueFlushInterval"];
				}
				return queueFlushInterval.GetValueOrDefault();
			}
			set
			{
				if (this.IsReadOnly())
				{
					this._queueFlushInterval = new int?(value);
					return;
				}
				base["queueFlushInterval"] = value;
			}
		}
		[ConfigurationProperty("maxHitsFromIndexingService", IsRequired = false, DefaultValue = 500)]
		public int MaxHitsFromIndexingService
		{
			get
			{
				return (int)base["maxHitsFromIndexingService"];
			}
			set
			{
				base["maxHitsFromIndexingService"] = value;
			}
		}
		[ConfigurationProperty("useIndexingServicePaging", IsRequired = false, DefaultValue = true)]
		public bool UseIndexingServicePaging
		{
			get
			{
				bool? useIndexServicePaging = this._useIndexServicePaging;
				if (!useIndexServicePaging.HasValue)
				{
					return (bool)base["useIndexingServicePaging"];
				}
				return useIndexServicePaging.GetValueOrDefault();
			}
			set
			{
				if (this.IsReadOnly())
				{
					this._useIndexServicePaging = new bool?(value);
					return;
				}
				base["useIndexingServicePaging"] = value;
			}
		}
        [ConfigurationProperty("dynamicDataStoreName", IsRequired = false, DefaultValue = SearchConstants.DynamicDataStoreName)]
		public string DynamicDataStoreName
		{
			get
			{
				return (string)base["dynamicDataStoreName"];
			}
			set
			{
				base["dynamicDataStoreName"] = value;
			}
		}
		[ConfigurationProperty("updateUriTemplate", IsRequired = false, DefaultValue = "/update/?accesskey={accesskey}")]
		public string UpdateUriTemplate
		{
			get
			{
				return (string)base["updateUriTemplate"];
			}
			set
			{
				base["updateUriTemplate"] = value;
			}
		}
        #region Customized
        // Hieu Le: Added {sort} to template
        [ConfigurationProperty("searchUriTemplate", IsRequired = false, DefaultValue = "/search/?q={q}&namedindexes={namedindexes}&offset={offset}&limit={limit}&format=xml&accesskey={accesskey}&sort={sort}")]
		public string SearchUriTemplate
		{
			get
			{
				return (string)base["searchUriTemplate"];
			}
			set
			{
				base["searchUriTemplate"] = value;
			}
		}
        #endregion
        [ConfigurationProperty("resetUriTemplate", IsRequired = false, DefaultValue = "/reset/?namedindex={namedindex}&accesskey={accesskey}")]
		public string ResetUriTemplate
		{
			get
			{
				return (string)base["resetUriTemplate"];
			}
			set
			{
				base["resetUriTemplate"] = value;
			}
		}
		[ConfigurationProperty("resetHttpMethod", IsRequired = false, DefaultValue = "POST")]
		public string ResetHttpMethod
		{
			get
			{
				return (string)base["resetHttpMethod"];
			}
			set
			{
				base["resetHttpMethod"] = value;
			}
		}
		[ConfigurationProperty("namedIndexesUriTemplate", IsRequired = false, DefaultValue = "/namedindexes/?accesskey={accesskey}")]
		public string NamedIndexesUriTemplate
		{
			get
			{
				return (string)base["namedIndexesUriTemplate"];
			}
			set
			{
				base["namedIndexesUriTemplate"] = value;
			}
		}
		[ConfigurationProperty("xmlQualifiedNamespace", IsRequired = false, DefaultValue = SearchConstants.XmlQualifiedNamespace)]
		public string XmlQualifiedNamespace
		{
			get
			{
				return (string)base["xmlQualifiedNamespace"];
			}
			set
			{
				base["xmlQualifiedNamespace"] = value;
			}
		}
		[ConfigurationProperty("syndicationFeedAttributeNameTotalHits", IsRequired = false, DefaultValue = "TotalHits")]
		public string SyndicationFeedAttributeNameTotalHits
		{
			get
			{
				return (string)base["syndicationFeedAttributeNameTotalHits"];
			}
			set
			{
				base["syndicationFeedAttributeNameTotalHits"] = value;
			}
		}
		[ConfigurationProperty("syndicationFeedAttributeNameVersion", IsRequired = false, DefaultValue = "Version")]
		public string SyndicationFeedAttributeNameVersion
		{
			get
			{
				return (string)base["syndicationFeedAttributeNameVersion"];
			}
			set
			{
				base["syndicationFeedAttributeNameVersion"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameCulture", IsRequired = false, DefaultValue = "Culture")]
		public string SyndicationItemAttributeNameCulture
		{
			get
			{
				return (string)base["syndicationItemAttributeNameCulture"];
			}
			set
			{
				base["syndicationItemAttributeNameCulture"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameType", IsRequired = false, DefaultValue = "Type")]
		public string SyndicationItemAttributeNameType
		{
			get
			{
				return (string)base["syndicationItemAttributeNameType"];
			}
			set
			{
				base["syndicationItemAttributeNameType"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameReferenceId", IsRequired = false, DefaultValue = "ReferenceId")]
		public string SyndicationItemAttributeNameReferenceId
		{
			get
			{
				return (string)base["syndicationItemAttributeNameReferenceId"];
			}
			set
			{
				base["syndicationItemAttributeNameReferenceId"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameItemStatus", IsRequired = false, DefaultValue = "ItemStatus")]
		public string SyndicationItemAttributeNameItemStatus
		{
			get
			{
				return (string)base["syndicationItemAttributeNameItemStatus"];
			}
			set
			{
				base["syndicationItemAttributeNameItemStatus"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemElementNameAcl", IsRequired = false, DefaultValue = "ACL"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Acl")]
		public string SyndicationItemElementNameAcl
		{
			get
			{
				return (string)base["syndicationItemElementNameAcl"];
			}
			set
			{
				base["syndicationItemElementNameAcl"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemElementNameVirtualPath", IsRequired = false, DefaultValue = "VirtualPath")]
		public string SyndicationItemElementNameVirtualPath
		{
			get
			{
				return (string)base["syndicationItemElementNameVirtualPath"];
			}
			set
			{
				base["syndicationItemElementNameVirtualPath"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemElementNameMetadata", IsRequired = false, DefaultValue = "Metadata")]
		public string SyndicationItemElementNameMetadata
		{
			get
			{
				return (string)base["syndicationItemElementNameMetadata"];
			}
			set
			{
				base["syndicationItemElementNameMetadata"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameBoostFactor", IsRequired = false, DefaultValue = "BoostFactor")]
		public string SyndicationItemAttributeNameBoostFactor
		{
			get
			{
				return (string)base["syndicationItemAttributeNameBoostFactor"];
			}
			set
			{
				base["syndicationItemAttributeNameBoostFactor"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameNamedIndex", IsRequired = false, DefaultValue = "NamedIndex")]
		public string SyndicationItemAttributeNameNamedIndex
		{
			get
			{
				return (string)base["syndicationItemAttributeNameNamedIndex"];
			}
			set
			{
				base["syndicationItemAttributeNameNamedIndex"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameIndexAction", IsRequired = false, DefaultValue = "IndexAction")]
		public string SyndicationItemAttributeNameIndexAction
		{
			get
			{
				return (string)base["syndicationItemAttributeNameIndexAction"];
			}
			set
			{
				base["syndicationItemAttributeNameIndexAction"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameAutoUpdateVirtualPath", IsRequired = false, DefaultValue = "AutoUpdateVirtualPath")]
		public string SyndicationItemAttributeNameAutoUpdateVirtualPath
		{
			get
			{
				return (string)base["syndicationItemAttributeNameAutoUpdateVirtualPath"];
			}
			set
			{
				base["syndicationItemAttributeNameAutoUpdateVirtualPath"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameVersion", IsRequired = false, DefaultValue = "Version")]
		public string SyndicationItemAttributeNameVersion
		{
			get
			{
				return (string)base["syndicationItemAttributeNameVersion"];
			}
			set
			{
				base["syndicationItemAttributeNameVersion"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameDataUri", IsRequired = false, DefaultValue = "DataUri")]
		public string SyndicationItemAttributeNameDataUri
		{
			get
			{
				return (string)base["syndicationItemAttributeNameDataUri"];
			}
			set
			{
				base["syndicationItemAttributeNameDataUri"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNameScore", IsRequired = false, DefaultValue = "Score")]
		public string SyndicationItemAttributeNameScore
		{
			get
			{
				return (string)base["syndicationItemAttributeNameScore"];
			}
			set
			{
				base["syndicationItemAttributeNameScore"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNamePublicationEnd", IsRequired = false, DefaultValue = "PublicationEnd")]
		public string SyndicationItemAttributeNamePublicationEnd
		{
			get
			{
				return (string)base["syndicationItemAttributeNamePublicationEnd"];
			}
			set
			{
				base["syndicationItemAttributeNamePublicationEnd"] = value;
			}
		}
		[ConfigurationProperty("syndicationItemAttributeNamePublicationStart", IsRequired = false, DefaultValue = "PublicationStart")]
		public string SyndicationItemAttributeNamePublicationStart
		{
			get
			{
				return (string)base["syndicationItemAttributeNamePublicationStart"];
			}
			set
			{
				base["syndicationItemAttributeNamePublicationStart"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameId", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_ID")]
		public string IndexingServiceFieldNameId
		{
			get
			{
				return (string)base["indexingServiceFieldNameId"];
			}
			set
			{
				base["indexingServiceFieldNameId"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameDefault", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_DEFAULT")]
		public string IndexingServiceFieldNameDefault
		{
			get
			{
				return (string)base["indexingServiceFieldNameDefault"];
			}
			set
			{
				base["indexingServiceFieldNameDefault"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameTitle", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_TITLE")]
		public string IndexingServiceFieldNameTitle
		{
			get
			{
				return (string)base["indexingServiceFieldNameTitle"];
			}
			set
			{
				base["indexingServiceFieldNameTitle"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameDisplayText", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_DISPLAYTEXT")]
		public string IndexingServiceFieldNameDisplayText
		{
			get
			{
				return (string)base["indexingServiceFieldNameDisplayText"];
			}
			set
			{
				base["indexingServiceFieldNameDisplayText"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameAuthors", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_AUTHORS")]
		public string IndexingServiceFieldNameAuthors
		{
			get
			{
				return (string)base["indexingServiceFieldNameAuthors"];
			}
			set
			{
				base["indexingServiceFieldNameAuthors"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameCreated", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_CREATED")]
		public string IndexingServiceFieldNameCreated
		{
			get
			{
				return (string)base["indexingServiceFieldNameCreated"];
			}
			set
			{
				base["indexingServiceFieldNameCreated"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameModified", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_MODIFIED")]
		public string IndexingServiceFieldNameModified
		{
			get
			{
				return (string)base["indexingServiceFieldNameModified"];
			}
			set
			{
				base["indexingServiceFieldNameModified"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameCategories", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_CATEGORIES")]
		public string IndexingServiceFieldNameCategories
		{
			get
			{
				return (string)base["indexingServiceFieldNameCategories"];
			}
			set
			{
				base["indexingServiceFieldNameCategories"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameAcl", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_ACL"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Acl")]
		public string IndexingServiceFieldNameAcl
		{
			get
			{
				return (string)base["indexingServiceFieldNameAcl"];
			}
			set
			{
				base["indexingServiceFieldNameAcl"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameVirtualPath", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_VIRTUALPATH")]
		public string IndexingServiceFieldNameVirtualPath
		{
			get
			{
				return (string)base["indexingServiceFieldNameVirtualPath"];
			}
			set
			{
				base["indexingServiceFieldNameVirtualPath"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameType", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_TYPE")]
		public string IndexingServiceFieldNameType
		{
			get
			{
				return (string)base["indexingServiceFieldNameType"];
			}
			set
			{
				base["indexingServiceFieldNameType"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameCulture", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_CULTURE")]
		public string IndexingServiceFieldNameCulture
		{
			get
			{
				return (string)base["indexingServiceFieldNameCulture"];
			}
			set
			{
				base["indexingServiceFieldNameCulture"] = value;
			}
		}
		[ConfigurationProperty("indexingServiceFieldNameItemStatus", IsRequired = false, DefaultValue = "EPISERVER_SEARCH_ITEMSTATUS")]
		public string IndexingServiceFieldNameItemStatus
		{
			get
			{
				return (string)base["indexingServiceFieldNameItemStatus"];
			}
			set
			{
				base["indexingServiceFieldNameItemStatus"] = value;
			}
		}
		[ConfigurationProperty("searchResultFilter", IsRequired = false)]
		public SearchResultFilterElement SearchResultFilterElement
		{
			get
			{
				return (SearchResultFilterElement)base["searchResultFilter"];
			}
			set
			{
				base["searchResultFilter"] = value;
			}
		}
		[ConfigurationProperty("namedIndexingServices", IsRequired = true)]
		public NamedIndexingServicesElement NamedIndexingServices
		{
			get
			{
				return (NamedIndexingServicesElement)base["namedIndexingServices"];
			}
			set
			{
				base["namedIndexingServices"] = value;
			}
		}
		[ConfigurationProperty("dequeuePageSize", IsRequired = false, DefaultValue = 50)]
		public int DequeuePageSize
		{
			get
			{
				return (int)base["dequeuePageSize"];
			}
			set
			{
				base["dequeuePageSize"] = value;
			}
		}
		[ConfigurationProperty("htmlStripTitle", IsRequired = false, DefaultValue = true)]
		public bool HtmlStripTitle
		{
			get
			{
				return (bool)base["htmlStripTitle"];
			}
			set
			{
				base["htmlStripTitle"] = value;
			}
		}
		[ConfigurationProperty("htmlStripDisplayText", IsRequired = false, DefaultValue = true)]
		public bool HtmlStripDisplayText
		{
			get
			{
				return (bool)base["htmlStripDisplayText"];
			}
			set
			{
				base["htmlStripDisplayText"] = value;
			}
		}
		[ConfigurationProperty("htmlStripMetadata", IsRequired = false, DefaultValue = true)]
		public bool HtmlStripMetadata
		{
			get
			{
				return (bool)base["htmlStripMetadata"];
			}
			set
			{
				base["htmlStripMetadata"] = value;
			}
		}
        internal static System.Configuration.Configuration GetConfigurationFromCurrentDomain()
		{
			ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap
			{
				ExeConfigFilename = System.AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
			};
			return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
		}
	}
}
