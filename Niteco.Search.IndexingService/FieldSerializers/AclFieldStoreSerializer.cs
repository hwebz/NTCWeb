using System.ServiceModel.Syndication;

namespace Niteco.Search.IndexingService.FieldSerializers
{
	internal class AclFieldStoreSerializer : TaggedFieldStoreSerializer
	{
		internal AclFieldStoreSerializer(SyndicationItem syndicationItem) : base(syndicationItem)
		{
		}
		internal AclFieldStoreSerializer(string indexFieldStoreValue) : base(indexFieldStoreValue)
		{
		}
		internal override string ToFieldStoreValue()
		{
			return base.ToFieldStoreString("ACL");
		}
		internal override void AddFieldStoreValueToSyndicationItem(SyndicationItem syndicationItem)
		{
			base.AddFieldStoreValueToSyndicationItem(syndicationItem, "ACL");
		}
	}
}
