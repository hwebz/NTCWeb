using System.ServiceModel.Syndication;

namespace Niteco.Search.IndexingService.FieldSerializers
{
	internal class VirtualPathFieldStoreSerializer : PipeSeparatedFieldStoreSerializer
	{
		internal VirtualPathFieldStoreSerializer(SyndicationItem syndicationItem) : base(syndicationItem)
		{
		}
		internal VirtualPathFieldStoreSerializer(string indexFieldStoreValue) : base(indexFieldStoreValue)
		{
		}
		internal override string ToFieldStoreValue()
		{
			return base.ToFieldStoreValue("VirtualPath");
		}
		internal override void AddFieldStoreValueToSyndicationItem(SyndicationItem syndicationItem)
		{
			base.AddFieldStoreValueToSyndicationItem(syndicationItem, "VirtualPath");
		}
	}
}
