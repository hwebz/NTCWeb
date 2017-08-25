using EPiServer.Data;
using EPiServer.Data.Dynamic;
using System;
namespace Niteco.Common.Search.Data
{
	[EPiServerDataStore(AutomaticallyRemapStore = true), EPiServerDataTable(TableName = "tblIndexRequestLog")]
	public class IndexRequestQueueItem : IDynamicData
	{
		public Identity Id
		{
			get;
			set;
		}
		public string IndexItemId
		{
			get;
			set;
		}
		[EPiServerDataIndex]
		public string NamedIndexingService
		{
			get;
			set;
		}
		public string SyndicationItemXml
		{
			get;
			set;
		}
		[EPiServerDataIndex]
		public System.DateTime Timestamp
		{
			get;
			set;
		}
		[EPiServerDataIndex]
		public string NamedIndex
		{
			get;
			set;
		}
	}
}
