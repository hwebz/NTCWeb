using System.ServiceModel.Syndication;

namespace Niteco.Search.IndexingService.FieldSerializers
{
	internal abstract class IndexFieldStoreSerializerBase
	{
		internal string FieldStoreValue
		{
			get;
			set;
		}
		internal SyndicationItem SyndicationItem
		{
			get;
			set;
		}
		internal IndexFieldStoreSerializerBase(SyndicationItem syndicationItem)
		{
			this.SyndicationItem = syndicationItem;
		}
		internal IndexFieldStoreSerializerBase(string fieldStoreValue)
		{
			this.FieldStoreValue = fieldStoreValue;
		}
		internal virtual string ToFieldStoreValue()
		{
			if (this.FieldStoreValue == null)
			{
				return "";
			}
			return this.FieldStoreValue;
		}
		internal virtual void AddFieldStoreValueToSyndicationItem(SyndicationItem syndicationItem)
		{
		}
	}
}
