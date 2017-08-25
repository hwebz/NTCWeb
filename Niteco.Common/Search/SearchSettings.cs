using EPiServer.Data.Dynamic;
using Niteco.Common.Search.Configuration;
using Niteco.Common.Search.Data;
using Niteco.Common.Search.Filter;
using Niteco.Common.Search.Queries.Lucene;
using log4net;
using System.Configuration;

namespace Niteco.Common.Search
{
	public sealed class SearchSettings
	{
		private static System.Collections.Generic.Dictionary<string, SearchResultFilterProvider> _searchResultFilterProviders;
		private static System.Collections.Generic.Dictionary<string, NamedIndexingServiceElement> _indexingServices;
		public static event System.EventHandler InitializationCompleted;
		public static SearchSection Config
		{
			get
			{
				return SearchSection.Instance ?? new SearchSection();
			}
		}
		public static System.Collections.Generic.Dictionary<string, NamedIndexingServiceElement> IndexingServices
		{
			get
			{
				return SearchSettings._indexingServices;
			}
		}
		public static ILog Log
		{
			get;
			set;
		}
		public static System.Collections.Generic.Dictionary<string, SearchResultFilterProvider> SearchResultFilterProviders
		{
			get
			{
				return SearchSettings._searchResultFilterProviders;
			}
		}
		private SearchSettings()
		{
		}
		static SearchSettings()
		{
			SearchSettings._searchResultFilterProviders = new System.Collections.Generic.Dictionary<string, SearchResultFilterProvider>();
			SearchSettings._indexingServices = new System.Collections.Generic.Dictionary<string, NamedIndexingServiceElement>();
		}
		public static DynamicDataStore GetDynamicDataStore()
		{
			return DynamicDataStoreFactory.Instance.GetStore(SearchSettings.Config.DynamicDataStoreName) ?? DynamicDataStoreFactory.Instance.CreateStore(SearchSettings.Config.DynamicDataStoreName, typeof(IndexRequestQueueItem));
		}
		internal static string GetFieldNameForField(Field field)
		{
			switch (field)
			{
			case Field.Default:
				return SearchSettings.Config.IndexingServiceFieldNameDefault;
			case Field.Id:
				return SearchSettings.Config.IndexingServiceFieldNameId;
			case Field.Title:
				return SearchSettings.Config.IndexingServiceFieldNameTitle;
			case Field.DisplayText:
				return SearchSettings.Config.IndexingServiceFieldNameDisplayText;
			case Field.Authors:
				return SearchSettings.Config.IndexingServiceFieldNameAuthors;
			case Field.Created:
				return SearchSettings.Config.IndexingServiceFieldNameCreated;
			case Field.Modified:
				return SearchSettings.Config.IndexingServiceFieldNameModified;
			case Field.Culture:
				return SearchSettings.Config.IndexingServiceFieldNameCulture;
			case Field.ItemType:
				return SearchSettings.Config.IndexingServiceFieldNameType;
			case Field.ItemStatus:
				return SearchSettings.Config.IndexingServiceFieldNameItemStatus;
			default:
				return SearchSettings.Config.IndexingServiceFieldNameDefault;
			}
		}
		internal static string GetIndexActionName(IndexAction indexAction)
		{
			switch (indexAction)
			{
			case IndexAction.Add:
				return "add";
			case IndexAction.Update:
				return "update";
			case IndexAction.Remove:
				return "remove";
			default:
				return "";
			}
		}
		internal static NamedIndexingServiceElement GetNamedIndexingServiceElement(string namedIndexingService)
		{
			if (string.IsNullOrEmpty(namedIndexingService))
			{
				namedIndexingService = SearchSettings.Config.NamedIndexingServices.DefaultService;
				if (string.IsNullOrEmpty(namedIndexingService))
				{
					throw new System.InvalidOperationException("Cannot fallback to default indexing service since it is not defined (defaultService in configuration)");
				}
			}
			NamedIndexingServiceElement result;
			if (!SearchSettings.IndexingServices.TryGetValue(namedIndexingService, out result))
			{
				throw new System.ArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, "The named indexing service \"{0}\" is not defined in the configuration", new object[]
				{
					namedIndexingService
				}));
			}
			return result;
		}
		internal static void LoadSearchResultFilterProviders(SearchResultFilterElement elem)
		{
			foreach (ProviderSettings providerSettings in elem.Providers)
			{
				System.Type type = System.Type.GetType(providerSettings.Type);
				if (type == null)
				{
					throw new System.ApplicationException(string.Format("The search result filter provider type does not exist for provider with name '{0}'.", providerSettings.Type));
				}
				System.Reflection.PropertyInfo property = type.GetProperty("Instance");
				if (property == null)
				{
					throw new System.ApplicationException(string.Format("The Instance property could not be found for provider with name '{0}'.", providerSettings.Type));
				}
				SearchResultFilterProvider searchResultFilterProvider = (SearchResultFilterProvider)property.GetValue(null, null);
				if (searchResultFilterProvider == null)
				{
					throw new System.ApplicationException(string.Format("The Instance property is null for provider with name '{0}'.", providerSettings.Type));
				}
				SearchSettings._searchResultFilterProviders.Add(providerSettings.Name, searchResultFilterProvider);
			}
		}
		internal static void LoadNamedIndexingServices(NamedIndexingServicesElement elem)
		{
			foreach (NamedIndexingServiceElement namedIndexingServiceElement in elem.NamedIndexingServices)
			{
				SearchSettings.IndexingServices.Add(namedIndexingServiceElement.Name, namedIndexingServiceElement);
			}
		}
		internal static void OnInitializationCompleted()
		{
			if (SearchSettings.InitializationCompleted != null)
			{
				SearchSettings.InitializationCompleted(null, new System.EventArgs());
			}
		}
	}
}
