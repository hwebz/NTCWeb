using System.Configuration;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using log4net;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Niteco.Search.IndexingService.Configuration;
using Niteco.Search.IndexingService.Custom;

namespace Niteco.Search.IndexingService
{
	internal class IndexingServiceSettings
	{
		internal const string TagsPrefix = "[[";
		internal const string TagsSuffix = "]]";
		internal const string DefaultFieldName = "EPISERVER_SEARCH_DEFAULT";
		internal const string IdFieldName = "EPISERVER_SEARCH_ID";
		internal const string TitleFieldName = "EPISERVER_SEARCH_TITLE";
		internal const string DisplayTextFieldName = "EPISERVER_SEARCH_DISPLAYTEXT";
		internal const string CreatedFieldName = "EPISERVER_SEARCH_CREATED";
		internal const string ModifiedFieldName = "EPISERVER_SEARCH_MODIFIED";
		internal const string PublicationEndFieldName = "EPISERVER_SEARCH_PUBLICATIONEND";
		internal const string PublicationStartFieldName = "EPISERVER_SEARCH_PUBLICATIONSTART";
		internal const string UriFieldName = "EPISERVER_SEARCH_URI";
		internal const string CategoriesFieldName = "EPISERVER_SEARCH_CATEGORIES";
		internal const string AuthorsFieldName = "EPISERVER_SEARCH_AUTHORS";
		internal const string CultureFieldName = "EPISERVER_SEARCH_CULTURE";
		internal const string TypeFieldName = "EPISERVER_SEARCH_TYPE";
		internal const string ReferenceIdFieldName = "EPISERVER_SEARCH_REFERENCEID";
		internal const string MetadataFieldName = "EPISERVER_SEARCH_METADATA";
		internal const string AclFieldName = "EPISERVER_SEARCH_ACL";
		internal const string VirtualPathFieldName = "EPISERVER_SEARCH_VIRTUALPATH";
		internal const string DataUriFieldName = "EPISERVER_SEARCH_DATAURI";
		internal const string AuthorStorageFieldName = "EPISERVER_SEARCH_AUTHORSTORAGE";
		internal const string NamedIndexFieldName = "EPISERVER_SEARCH_NAMEDINDEX";
		internal const string ItemStatusFieldName = "EPISERVER_SEARCH_ITEMSTATUS";
        internal const string XmlQualifiedNamespace = IndexingServiceConstants.Namespace;
		internal const string SyndicationItemAttributeNameCulture = "Culture";
		internal const string SyndicationItemAttributeNameType = "Type";
		internal const string SyndicationItemElementNameMetadata = "Metadata";
		internal const string SyndicationItemAttributeNameNamedIndex = "NamedIndex";
		internal const string SyndicationItemAttributeNameBoostFactor = "BoostFactor";
		internal const string SyndicationItemAttributeNameIndexAction = "IndexAction";
		internal const string SyndicationItemAttributeNameReferenceId = "ReferenceId";
		internal const string SyndicationItemElementNameAcl = "ACL";
		internal const string SyndicationItemAttributeNameDataUri = "DataUri";
		internal const string SyndicationItemElementNameVirtualPath = "VirtualPath";
		internal const string SyndicationItemAttributeNameScore = "Score";
		internal const string SyndicationFeedAttributeNameVersion = "Version";
		internal const string SyndicationFeedAttributeNameTotalHits = "TotalHits";
		internal const string SyndicationItemAttributeNamePublicationEnd = "PublicationEnd";
		internal const string SyndicationItemAttributeNamePublicationStart = "PublicationStart";
		internal const string SyndicationItemAttributeNameItemStatus = "ItemStatus";
		internal const string SyndicationItemAttributeNameAutoUpdateVirtualPath = "AutoUpdateVirtualPath";
		internal const string RefIndexSuffix = "_ref";
		private static string _defaultIndexName;
		private static Analyzer _analyzer;
		private static System.Collections.Generic.Dictionary<string, ClientElement> _clientElements;
		private static System.Collections.Generic.Dictionary<string, NamedIndexElement> _namedIndexElements;
		private static System.Collections.Generic.Dictionary<string, Lucene.Net.Store.Directory> _namedIndexDirectories;
		private static System.Collections.Generic.Dictionary<string, Lucene.Net.Store.Directory> _referenceIndexDirectories;
		private static System.Collections.Generic.Dictionary<string, System.IO.DirectoryInfo> _mainDirectoryInfos;
		private static System.Collections.Generic.Dictionary<string, System.IO.DirectoryInfo> _referenceDirectoryInfos;
		private static System.Collections.Generic.Dictionary<string, int> _indexWriteCounters;
		private static System.Collections.Generic.Dictionary<string, Analyzer> _indexAnalyzers;
		private static System.Collections.Generic.Dictionary<string, ReaderWriterLockSlim> _readerWriterLocks;
		private static System.Collections.Generic.Dictionary<string, FieldProperties> _fieldProperties;
		private static System.Collections.Generic.IList<string> _lowercaseFields;
		internal static Lucene.Net.Util.Version LuceneVersion
		{
			get;
			set;
		}
		internal static int MaxDisplayTextLength
		{
			get;
			set;
		}
		internal static int MaxHitsForSearchResults
		{
			get;
			set;
		}
		internal static int MaxHitsForReferenceSearch
		{
			get;
			set;
		}
		internal static Lucene.Net.Store.Directory DefaultDirectory
		{
			get;
			private set;
		}
		internal static Lucene.Net.Store.Directory DefaultReferenceDirectory
		{
			get;
			private set;
		}
		internal static ILog IndexingServiceServiceLog
		{
			get;
			set;
		}
		internal static Analyzer Analyzer
		{
			get
			{
				return IndexingServiceSettings._analyzer;
			}
		}
		internal static System.Collections.Generic.Dictionary<string, ReaderWriterLockSlim> ReaderWriterLocks
		{
			get
			{
				return IndexingServiceSettings._readerWriterLocks;
			}
		}
		internal static System.Collections.Generic.Dictionary<string, NamedIndexElement> NamedIndexElements
		{
			get
			{
				return IndexingServiceSettings._namedIndexElements;
			}
		}
		internal static System.Collections.Generic.Dictionary<string, Lucene.Net.Store.Directory> NamedIndexDirectories
		{
			get
			{
				return IndexingServiceSettings._namedIndexDirectories;
			}
		}
		internal static System.Collections.Generic.Dictionary<string, Lucene.Net.Store.Directory> ReferenceIndexDirectories
		{
			get
			{
				return IndexingServiceSettings._referenceIndexDirectories;
			}
		}
		internal static System.Collections.Generic.Dictionary<string, System.IO.DirectoryInfo> MainDirectoryInfos
		{
			get
			{
				return IndexingServiceSettings._mainDirectoryInfos;
			}
		}
		internal static System.Collections.Generic.Dictionary<string, System.IO.DirectoryInfo> ReferenceDirectoryInfos
		{
			get
			{
				return IndexingServiceSettings._referenceDirectoryInfos;
			}
		}
		internal static System.Collections.Generic.Dictionary<string, FieldProperties> FieldProperties
		{
			get
			{
				return IndexingServiceSettings._fieldProperties;
			}
		}
		internal static System.Collections.Generic.IList<string> LowercaseFields
		{
			get
			{
				return IndexingServiceSettings._lowercaseFields;
			}
		}
		internal static string DefaultIndexName
		{
			get
			{
				return IndexingServiceSettings._defaultIndexName;
			}
			set
			{
				IndexingServiceSettings._defaultIndexName = value;
				if (IndexingServiceSettings.NamedIndexDirectories[IndexingServiceSettings.DefaultIndexName] != null)
				{
					IndexingServiceSettings.DefaultDirectory = IndexingServiceSettings.NamedIndexDirectories[value];
				}
			}
		}
		internal static System.Collections.Generic.Dictionary<string, ClientElement> ClientElements
		{
			get
			{
				return IndexingServiceSettings._clientElements;
			}
		}
		static IndexingServiceSettings()
		{
			IndexingServiceSettings._clientElements = new System.Collections.Generic.Dictionary<string, ClientElement>();
			IndexingServiceSettings._namedIndexElements = new System.Collections.Generic.Dictionary<string, NamedIndexElement>();
			IndexingServiceSettings._namedIndexDirectories = new System.Collections.Generic.Dictionary<string, Lucene.Net.Store.Directory>();
			IndexingServiceSettings._referenceIndexDirectories = new System.Collections.Generic.Dictionary<string, Lucene.Net.Store.Directory>();
			IndexingServiceSettings._mainDirectoryInfos = new System.Collections.Generic.Dictionary<string, System.IO.DirectoryInfo>();
			IndexingServiceSettings._referenceDirectoryInfos = new System.Collections.Generic.Dictionary<string, System.IO.DirectoryInfo>();
			IndexingServiceSettings._indexWriteCounters = new System.Collections.Generic.Dictionary<string, int>();
			IndexingServiceSettings._indexAnalyzers = new System.Collections.Generic.Dictionary<string, Analyzer>();
			IndexingServiceSettings._readerWriterLocks = new System.Collections.Generic.Dictionary<string, ReaderWriterLockSlim>();
			IndexingServiceSettings._fieldProperties = new System.Collections.Generic.Dictionary<string, FieldProperties>();
			IndexingServiceSettings._lowercaseFields = new System.Collections.Generic.List<string>
			{
				DefaultFieldName,
				IndexingServiceSettings.TitleFieldName,
				IndexingServiceSettings.DisplayTextFieldName,
				IndexingServiceSettings.AuthorsFieldName
			};
			IndexingServiceSettings.Init();
		}
		public void Dispose()
		{
		}
		private static void Init()
		{
			IndexingServiceSettings.IndexingServiceServiceLog = LogManager.GetLogger(typeof(Niteco.Search.IndexingService.IndexingService).Name);
			IndexingServiceSettings.LuceneVersion = Lucene.Net.Util.Version.LUCENE_29;
			IndexingServiceSettings.LoadConfiguration();
			IndexingServiceSettings.LoadIndexes();
			IndexingServiceSettings.LoadFieldProperties();
			IndexingServiceSettings.LoadAnalyzer();
			IndexingServiceSettings.IndexingServiceServiceLog.Info("EPiServer Indexing Service Started!");
		}
		internal static void SetResponseHeaderStatusCode(int statusCode)
		{
			HttpResponseMessageProperty httpResponseMessageProperty = new HttpResponseMessageProperty();
			httpResponseMessageProperty.StatusCode = (HttpStatusCode)statusCode;
			OperationContext.Current.OutgoingMessageProperties[HttpResponseMessageProperty.Name] = httpResponseMessageProperty;
		}
		internal static void HandleServiceError(string errorMessage)
		{
			IndexingServiceSettings.IndexingServiceServiceLog.Error(errorMessage);
			Niteco.Search.IndexingService.IndexingService.OnInternalServerError(null, new InternalServerErrorEventArgs(errorMessage));
			IndexingServiceSettings.SetResponseHeaderStatusCode(500);
		}
		private static void LoadConfiguration()
		{
			IndexingServiceSection indexingServiceSection = ConfigurationManager.GetSection(IndexingServiceConstants.ConfigurationSectionName) as IndexingServiceSection;
			if (indexingServiceSection != null)
			{
				IndexingServiceSettings.MaxHitsForSearchResults = indexingServiceSection.MaxHitsForSearchResults;
				IndexingServiceSettings.MaxHitsForReferenceSearch = indexingServiceSection.MaxHitsForReferenceSearch;
				IndexingServiceSettings.MaxDisplayTextLength = indexingServiceSection.MaxDisplayTextLength;
				foreach (ClientElement clientElement in indexingServiceSection.Clients)
				{
					IndexingServiceSettings.ClientElements.Add(clientElement.Name, clientElement);
				}
				foreach (NamedIndexElement namedIndexElement in indexingServiceSection.NamedIndexesElement.NamedIndexes)
				{
					IndexingServiceSettings.NamedIndexElements.Add(namedIndexElement.Name, namedIndexElement);
				}
				IndexingServiceSettings._defaultIndexName = indexingServiceSection.NamedIndexesElement.DefaultIndex;
			}
		}
		private static void LoadIndexes()
		{
			foreach (NamedIndexElement current in IndexingServiceSettings.NamedIndexElements.Values)
			{
				System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(System.IO.Path.Combine(current.GetDirectoryPath(), "Main"));
				System.IO.DirectoryInfo directoryInfo2 = new System.IO.DirectoryInfo(System.IO.Path.Combine(current.GetDirectoryPath(), "Ref"));
				IndexingServiceSettings.ReaderWriterLocks.Add(current.Name, new ReaderWriterLockSlim());
				IndexingServiceSettings.ReaderWriterLocks.Add(current.Name + "_ref", new ReaderWriterLockSlim());
				try
				{
					if (!directoryInfo.Exists)
					{
						directoryInfo.Create();
						Lucene.Net.Store.Directory value = IndexingServiceHandler.CreateIndex(current.Name, directoryInfo);
						IndexingServiceSettings.NamedIndexDirectories.Add(current.Name, value);
					}
					else
					{
						IndexingServiceSettings.NamedIndexDirectories.Add(current.Name, FSDirectory.Open(directoryInfo));
					}
					if (!directoryInfo2.Exists)
					{
						directoryInfo2.Create();
						Lucene.Net.Store.Directory value2 = IndexingServiceHandler.CreateIndex(current.Name + "_ref", directoryInfo2);
						IndexingServiceSettings.ReferenceIndexDirectories.Add(current.Name, value2);
					}
					else
					{
						IndexingServiceSettings.ReferenceIndexDirectories.Add(current.Name, FSDirectory.Open(directoryInfo2));
					}
					IndexingServiceSettings.MainDirectoryInfos.Add(current.Name, directoryInfo);
					IndexingServiceSettings.ReferenceDirectoryInfos.Add(current.Name, directoryInfo2);
					IndexingServiceSettings.DefaultDirectory = IndexingServiceSettings.NamedIndexDirectories[IndexingServiceSettings.DefaultIndexName];
					IndexingServiceSettings.DefaultReferenceDirectory = IndexingServiceSettings.NamedIndexDirectories[IndexingServiceSettings.DefaultIndexName];
				}
				catch (System.Exception ex)
				{
					IndexingServiceSettings.IndexingServiceServiceLog.Fatal(string.Format("Failed to load or create index: \"{0}\". Message: {1}", current.Name, ex.Message), ex);
					Niteco.Search.IndexingService.IndexingService.OnInternalServerError(null, new InternalServerErrorEventArgs(string.Format("Failed to load or create index: {0}. Message: {1}{2}{3}", new object[]
					{
						current.Name,
						ex.Message,
						System.Environment.NewLine,
						ex.StackTrace
					})));
				}
			}
		}
		private static void LoadFieldProperties()
		{
			IndexingServiceSettings._fieldProperties.Add(IdFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.TitleFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.DisplayTextFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.ANALYZED
			});
            IndexingServiceSettings._fieldProperties.Add(CreatedFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.ModifiedFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.PublicationEndFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.PublicationStartFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.UriFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NO
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.MetadataFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NO
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.CategoriesFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.CultureFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.AuthorsFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.TypeFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.ReferenceIdFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.AclFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.VirtualPathFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.AuthorStorageFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.NamedIndexFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(DefaultFieldName, new FieldProperties
			{
				FieldStore = Field.Store.NO,
				FieldIndex = Field.Index.ANALYZED
			});
			IndexingServiceSettings._fieldProperties.Add(IndexingServiceSettings.ItemStatusFieldName, new FieldProperties
			{
				FieldStore = Field.Store.YES,
				FieldIndex = Field.Index.NOT_ANALYZED
			});
		}
		private static void LoadAnalyzer()
		{
			string[] stopWords = new string[0];
			PerFieldAnalyzerWrapper perFieldAnalyzerWrapper = new PerFieldAnalyzerWrapper(new StandardAnalyzer(IndexingServiceSettings.LuceneVersion, StopFilter.MakeStopSet(stopWords)));
            perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.IdFieldName, new KeywordAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.CultureFieldName, new KeywordAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.ReferenceIdFieldName, new KeywordAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.AuthorStorageFieldName, new KeywordAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.CategoriesFieldName, new WhitespaceAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.AclFieldName, new WhitespaceAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.VirtualPathFieldName, new WhitespaceAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.TypeFieldName, new WhitespaceAnalyzer());
            perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.CreatedFieldName, new WhitespaceAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.ModifiedFieldName, new WhitespaceAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.PublicationEndFieldName, new WhitespaceAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.PublicationStartFieldName, new WhitespaceAnalyzer());
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.ItemStatusFieldName, new WhitespaceAnalyzer());
			Analyzer analyzer = new StandardAnalyzer(IndexingServiceSettings.LuceneVersion, StopFilter.MakeStopSet(stopWords));
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.TitleFieldName, analyzer);
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.DisplayTextFieldName, analyzer);
			perFieldAnalyzerWrapper.AddAnalyzer(IndexingServiceSettings.AuthorsFieldName, analyzer);
			perFieldAnalyzerWrapper.AddAnalyzer(DefaultFieldName, analyzer);
			IndexingServiceSettings._analyzer = perFieldAnalyzerWrapper;
		}
	}
}
