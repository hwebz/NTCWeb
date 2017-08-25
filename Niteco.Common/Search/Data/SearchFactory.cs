using EPiServer.Data;
using EPiServer.Data.Dynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Xml;
namespace Niteco.Common.Search.Data
{
	public sealed class SearchFactory
	{
		private SearchFactory()
		{
		}
		internal static void AddToQueue(IndexRequestItem item, string namedIndexingService)
		{
			if (string.IsNullOrEmpty(namedIndexingService))
			{
				namedIndexingService = SearchSettings.Config.NamedIndexingServices.DefaultService;
			}

			SearchSettings.GetDynamicDataStore().Save(new IndexRequestQueueItem
			{
				IndexItemId = item.Id,
				NamedIndex = item.NamedIndex,
				NamedIndexingService = namedIndexingService,
				SyndicationItemXml = item.ToSyndicationItemXml(),
				Timestamp = System.DateTime.Now
			});
		}
		internal static SyndicationFeed GetUnprocessedQueueItems(string namedIndexingService, int pageSize, out System.Collections.ObjectModel.Collection<Identity> ids)
		{
			SyndicationFeed syndicationFeed = new SyndicationFeed();
			System.Collections.Generic.List<SyndicationItem> list = new System.Collections.Generic.List<SyndicationItem>();
			ids = new System.Collections.ObjectModel.Collection<Identity>();
			IQueryable<IndexRequestQueueItem> queryable = (
				from queueItem in SearchSettings.GetDynamicDataStore().Items<IndexRequestQueueItem>()
				where queueItem.NamedIndexingService == namedIndexingService
				orderby queueItem.Timestamp
				select queueItem).Take(pageSize);
			foreach (IndexRequestQueueItem current in queryable)
			{
				list.Add(SearchFactory.ConstructSyndicationItem(current));
				ids.Add(current.Id);
			}
			System.Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "EPiServer.Search v.{0}.{1}.{2}.{3}", new object[]
			{
				version.Major.ToString(System.Globalization.CultureInfo.InvariantCulture),
				version.Minor.ToString(System.Globalization.CultureInfo.InvariantCulture),
				version.Build.ToString(System.Globalization.CultureInfo.InvariantCulture),
				version.Revision.ToString(System.Globalization.CultureInfo.InvariantCulture)
			});
			syndicationFeed.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameVersion, SearchSettings.Config.XmlQualifiedNamespace), value);
			syndicationFeed.Items = list;
			return syndicationFeed;
		}
		internal static void RemoveProcessedQueueItems(System.Collections.ObjectModel.Collection<Identity> ids)
		{
			using (DynamicDataStore dynamicDataStore = SearchSettings.GetDynamicDataStore())
			{
				foreach (Identity current in ids)
				{
					dynamicDataStore.Delete(current);
				}
			}
		}
		internal static void TruncateQueue()
		{
			SearchSettings.GetDynamicDataStore().DeleteAll();
		}
		internal static void TruncateQueue(string namedIndexingService, string namedIndex)
		{
			IQueryable<IndexRequestQueueItem> queryable = null;
			string namedService = (!string.IsNullOrEmpty(namedIndexingService)) ? namedIndexingService : SearchSettings.Config.NamedIndexingServices.DefaultService;
			do
			{
				queryable = (
					from queueItem in SearchSettings.GetDynamicDataStore().Items<IndexRequestQueueItem>()
					where queueItem.NamedIndexingService == namedService && queueItem.NamedIndex == namedIndex
					select queueItem).Take(100);
				foreach (IndexRequestQueueItem current in queryable.ToList<IndexRequestQueueItem>())
				{
					SearchSettings.GetDynamicDataStore().Delete(current.Id);
				}
			}
			while (queryable != null && queryable.Count<IndexRequestQueueItem>() != 0);
		}
		private static SyndicationItem ConstructSyndicationItem(IndexRequestQueueItem queueItem)
		{
			SyndicationItem result;
			using (System.IO.StringReader stringReader = new System.IO.StringReader(queueItem.SyndicationItemXml))
			{
				XmlReader reader = new XmlTextReader(stringReader)
				{
					Normalization = false
				};
				SyndicationItem syndicationItem = SyndicationItem.Load(reader);
				result = syndicationItem;
			}
			return result;
		}
	}
}
